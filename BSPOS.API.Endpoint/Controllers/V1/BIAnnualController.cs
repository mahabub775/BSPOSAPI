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
public partial class BIAnnualController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<BIAnnualController> _logger;
	private readonly IConfiguration _config;
	private readonly IBIAnnualRepository _BIAnnualRepository;
	private readonly ICsvExporter _csvExporter;

	public BIAnnualController(ISecurityHelper securityHelper, ILogger<BIAnnualController> logger, IConfiguration config, IBIAnnualRepository BIAnnualRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._BIAnnualRepository = BIAnnualRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetBIAnnuals(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.BIAnnual_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _BIAnnualRepository.GetBIAnnuals(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.BIAnnual_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctBIAnnuals"), AllowAnonymous]
	public Task<IActionResult> GetDistinctBIAnnuals() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _BIAnnualRepository.GetDistinctBIAnnuals();
		if (result == null)
			return NotFound(ValidationMessages.BIAnnual_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetBIAnnualById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.BIAnnual_InvalidId, id));
		#endregion

		var result = await _BIAnnualRepository.GetBIAnnualById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.BIAnnual_NotFoundId, id));

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

		var result = await _BIAnnualRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.BIAnnual_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertBIAnnual([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BIAnnualModel BIAnnual = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BIAnnualModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), BIAnnual.BIAnnualName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (BIAnnual == null) return BadRequest(ValidationMessages.BIAnnual_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingBIAnnual = await _BIAnnualRepository.GetBIAnnualByName(BIAnnual.BIAnnualName);
		if (existingBIAnnual != null)
			return BadRequest(String.Format(ValidationMessages.BIAnnual_Duplicate, BIAnnual.BIAnnualName));
		#endregion

		int insertedBIAnnualId = await _BIAnnualRepository.InsertBIAnnual(BIAnnual, logModel);
		return Created(nameof(GetBIAnnualById), new { id = insertedBIAnnualId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateBIAnnual(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BIAnnualModel BIAnnual = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BIAnnualModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.BIAnnual_InvalidId, id));
		if (BIAnnual == null) return BadRequest(ValidationMessages.BIAnnual_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != BIAnnual.BIAnnualId) return BadRequest(ValidationMessages.BIAnnual_Mismatch);

		var BIAnnualToUpdate = await _BIAnnualRepository.GetBIAnnualById(id);
		if (BIAnnualToUpdate == null)
			return NotFound(String.Format(ValidationMessages.BIAnnual_NotFoundId, id));
		#endregion

		await _BIAnnualRepository.UpdateBIAnnual(BIAnnual, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteBIAnnual(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.BIAnnual_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var BIAnnualToDelete = await _BIAnnualRepository.GetBIAnnualById(id);
		if (BIAnnualToDelete == null)
			return NotFound(String.Format(ValidationMessages.BIAnnual_NotFoundId, id));
		#endregion

		await _BIAnnualRepository.DeleteBIAnnual(id, logModel);
		return NoContent(); // success
	});
}