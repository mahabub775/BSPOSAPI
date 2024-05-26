using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using SMS.Core.Constant;
using SMS.Core.Contract.Infrastructure;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class RankController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<RankController> _logger;
	private readonly IConfiguration _config;
	private readonly IRankRepository _RankRepository;
	private readonly ICsvExporter _csvExporter;

	public RankController(ISecurityHelper securityHelper, ILogger<RankController> logger, IConfiguration config, IRankRepository RankRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._RankRepository = RankRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetRanks(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Rank_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _RankRepository.GetRanks(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Rank_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctRanks"), AllowAnonymous]
	public Task<IActionResult> GetDistinctRanks() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _RankRepository.GetDistinctRanks();
		if (result == null)
			return NotFound(ValidationMessages.Rank_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetRankById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Rank_InvalidId, id));
		#endregion

		var result = await _RankRepository.GetRankById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Rank_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("Export"), AllowAnonymous]
	public Task<IActionResult> Export() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _RankRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Rank_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertRank([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		RankModel Rank = PostData["Data"] == null ? null : JsonSerializer.Deserialize<RankModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Rank.RankName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Rank == null) return BadRequest(ValidationMessages.Rank_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingRank = await _RankRepository.GetRankByName(Rank.RankName);
		if (existingRank != null)
			return BadRequest(String.Format(ValidationMessages.Rank_Duplicate, Rank.RankName));
		#endregion

		int insertedRankId = await _RankRepository.InsertRank(Rank, logModel);
		return Created(nameof(GetRankById), new { id = insertedRankId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateRank(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		RankModel Rank = PostData["Data"] == null ? null : JsonSerializer.Deserialize<RankModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Rank_InvalidId, id));
		if (Rank == null) return BadRequest(ValidationMessages.Rank_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Rank.RankId) return BadRequest(ValidationMessages.Rank_Mismatch);

		var RankToUpdate = await _RankRepository.GetRankById(id);
		if (RankToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Rank_NotFoundId, id));
		#endregion

		await _RankRepository.UpdateRank(Rank, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteRank(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Rank_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var RankToDelete = await _RankRepository.GetRankById(id);
		if (RankToDelete == null)
			return NotFound(String.Format(ValidationMessages.Rank_NotFoundId, id));
		#endregion

		await _RankRepository.DeleteRank(id, logModel);
		return NoContent(); // success
	});
}