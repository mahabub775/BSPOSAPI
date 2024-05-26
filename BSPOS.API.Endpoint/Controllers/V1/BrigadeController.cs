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
public partial class BrigadeController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<BrigadeController> _logger;
	private readonly IConfiguration _config;
	private readonly IBrigadeRepository _BrigadeRepository;
	private readonly ICsvExporter _csvExporter;

	public BrigadeController(ISecurityHelper securityHelper, ILogger<BrigadeController> logger, IConfiguration config, IBrigadeRepository BrigadeRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._BrigadeRepository = BrigadeRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetBrigades(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Brigade_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _BrigadeRepository.GetBrigades(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Brigade_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctBrigades"), AllowAnonymous]
	public Task<IActionResult> GetDistinctBrigades() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _BrigadeRepository.GetDistinctBrigades();
		if (result == null)
			return NotFound(ValidationMessages.Brigade_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetBrigadeById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Brigade_InvalidId, id));
		#endregion

		var result = await _BrigadeRepository.GetBrigadeById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Brigade_NotFoundId, id));

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

		var result = await _BrigadeRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Brigade_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertBrigade([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BrigadeModel Brigade = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BrigadeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Brigade.BrigadeName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Brigade == null) return BadRequest(ValidationMessages.Brigade_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingBrigade = await _BrigadeRepository.GetBrigadeByName(Brigade.BrigadeName);
		if (existingBrigade != null)
			return BadRequest(String.Format(ValidationMessages.Brigade_Duplicate, Brigade.BrigadeName));
		#endregion

		int insertedBrigadeId = await _BrigadeRepository.InsertBrigade(Brigade, logModel);
		return Created(nameof(GetBrigadeById), new { id = insertedBrigadeId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateBrigade(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BrigadeModel Brigade = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BrigadeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Brigade_InvalidId, id));
		if (Brigade == null) return BadRequest(ValidationMessages.Brigade_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Brigade.BrigadeId) return BadRequest(ValidationMessages.Brigade_Mismatch);

		var BrigadeToUpdate = await _BrigadeRepository.GetBrigadeById(id);
		if (BrigadeToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Brigade_NotFoundId, id));
		#endregion

		await _BrigadeRepository.UpdateBrigade(Brigade, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteBrigade(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Brigade_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var BrigadeToDelete = await _BrigadeRepository.GetBrigadeById(id);
		if (BrigadeToDelete == null)
			return NotFound(String.Format(ValidationMessages.Brigade_NotFoundId, id));
		#endregion

		await _BrigadeRepository.DeleteBrigade(id, logModel);
		return NoContent(); // success
	});
}