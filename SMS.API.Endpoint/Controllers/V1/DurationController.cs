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
public partial class DurationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<DurationController> _logger;
	private readonly IConfiguration _config;
	private readonly IDurationRepository _DurationRepository;
	private readonly ICsvExporter _csvExporter;

	public DurationController(ISecurityHelper securityHelper, ILogger<DurationController> logger, IConfiguration config, IDurationRepository DurationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._DurationRepository = DurationRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetDurations(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Duration_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _DurationRepository.GetDurations(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Duration_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctDurations"), AllowAnonymous]
	public Task<IActionResult> GetDistinctDurations() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _DurationRepository.GetDistinctDurations();
		if (result == null)
			return NotFound(ValidationMessages.Duration_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetDurationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Duration_InvalidId, id));
		#endregion

		var result = await _DurationRepository.GetDurationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Duration_NotFoundId, id));

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

		var result = await _DurationRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Duration_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertDuration([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DurationModel Duration = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DurationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Duration.DurationName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Duration == null) return BadRequest(ValidationMessages.Duration_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingDuration = await _DurationRepository.GetDurationByName(Duration.DurationName);
		if (existingDuration != null)
			return BadRequest(String.Format(ValidationMessages.Duration_Duplicate, Duration.DurationName));
		#endregion

		int insertedDurationId = await _DurationRepository.InsertDuration(Duration, logModel);
		return Created(nameof(GetDurationById), new { id = insertedDurationId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateDuration(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DurationModel Duration = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DurationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Duration_InvalidId, id));
		if (Duration == null) return BadRequest(ValidationMessages.Duration_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Duration.DurationId) return BadRequest(ValidationMessages.Duration_Mismatch);

		var DurationToUpdate = await _DurationRepository.GetDurationById(id);
		if (DurationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Duration_NotFoundId, id));
		#endregion

		await _DurationRepository.UpdateDuration(Duration, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteDuration(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Duration_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var DurationToDelete = await _DurationRepository.GetDurationById(id);
		if (DurationToDelete == null)
			return NotFound(String.Format(ValidationMessages.Duration_NotFoundId, id));
		#endregion

		await _DurationRepository.DeleteDuration(id, logModel);
		return NoContent(); // success
	});
}