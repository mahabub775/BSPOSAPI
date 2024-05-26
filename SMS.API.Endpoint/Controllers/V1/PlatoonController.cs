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
public partial class PlatoonController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<PlatoonController> _logger;
	private readonly IConfiguration _config;
	private readonly IPlatoonRepository _PlatoonRepository;
	private readonly ICsvExporter _csvExporter;

	public PlatoonController(ISecurityHelper securityHelper, ILogger<PlatoonController> logger, IConfiguration config, IPlatoonRepository PlatoonRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._PlatoonRepository = PlatoonRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetPlatoons(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Platoon_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _PlatoonRepository.GetPlatoons(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Platoon_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctPlatoons"), AllowAnonymous]
	public Task<IActionResult> GetDistinctPlatoons() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _PlatoonRepository.GetDistinctPlatoons();
		if (result == null)
			return NotFound(ValidationMessages.Platoon_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetPlatoonById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Platoon_InvalidId, id));
		#endregion

		var result = await _PlatoonRepository.GetPlatoonById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Platoon_NotFoundId, id));

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

		var result = await _PlatoonRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Platoon_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertPlatoon([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		PlatoonModel Platoon = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PlatoonModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Platoon.PlatoonName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Platoon == null) return BadRequest(ValidationMessages.Platoon_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingPlatoon = await _PlatoonRepository.GetPlatoonByName(Platoon.PlatoonName);
		if (existingPlatoon != null)
			return BadRequest(String.Format(ValidationMessages.Platoon_Duplicate, Platoon.PlatoonName));
		#endregion

		int insertedPlatoonId = await _PlatoonRepository.InsertPlatoon(Platoon, logModel);
		return Created(nameof(GetPlatoonById), new { id = insertedPlatoonId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdatePlatoon(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		PlatoonModel Platoon = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PlatoonModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Platoon_InvalidId, id));
		if (Platoon == null) return BadRequest(ValidationMessages.Platoon_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Platoon.PlatoonId) return BadRequest(ValidationMessages.Platoon_Mismatch);

		var PlatoonToUpdate = await _PlatoonRepository.GetPlatoonById(id);
		if (PlatoonToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Platoon_NotFoundId, id));
		#endregion

		await _PlatoonRepository.UpdatePlatoon(Platoon, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeletePlatoon(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Platoon_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var PlatoonToDelete = await _PlatoonRepository.GetPlatoonById(id);
		if (PlatoonToDelete == null)
			return NotFound(String.Format(ValidationMessages.Platoon_NotFoundId, id));
		#endregion

		await _PlatoonRepository.DeletePlatoon(id, logModel);
		return NoContent(); // success
	});
}