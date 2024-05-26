using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using SMS.API.Persistence;
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
public partial class CivilInstitutionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CivilInstitutionController> _logger;
	private readonly IConfiguration _config;
	private readonly ICivilInstitutionRepository _CivilInstitutionRepository;
	private readonly ICsvExporter _csvExporter;

	public CivilInstitutionController(ISecurityHelper securityHelper, ILogger<CivilInstitutionController> logger, IConfiguration config, ICivilInstitutionRepository CivilInstitutionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CivilInstitutionRepository = CivilInstitutionRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCivilInstitutions(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.CivilInstitution_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CivilInstitutionRepository.GetCivilInstitutions(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.CivilInstitution_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCivilInstitutions"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCivilInstitutions() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CivilInstitutionRepository.GetDistinctCivilInstitutions();
		if (result == null)
			return NotFound(ValidationMessages.CivilInstitution_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCivilInstitutionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CivilInstitution_InvalidId, id));
		#endregion

		var result = await _CivilInstitutionRepository.GetCivilInstitutionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CivilInstitution_NotFoundId, id));

		return Ok(result);
	});


	[HttpGet("GetTopCivilEducations"), AllowAnonymous]
	public Task<IActionResult> GetTopCivilEducations() =>
TryCatch(async () =>
{
	#region Validation
	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	{
		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
			return Unauthorized(ValidationMessages.InvalidHash);
	}

	#endregion

	var result = await _CivilInstitutionRepository.GetTopCivilEducations();
	if (result == null)
		return NotFound(ValidationMessages.CivilInstitution_NotFoundList);

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

		var result = await _CivilInstitutionRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.CivilInstitution_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCivilInstitution([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CivilInstitutionModel CivilInstitution = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CivilInstitutionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CivilInstitution.InstitutionName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CivilInstitution == null) return BadRequest(ValidationMessages.CivilInstitution_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCivilInstitution = await _CivilInstitutionRepository.GetCivilInstitutionByName(CivilInstitution.InstitutionName);
		if (existingCivilInstitution != null)
			//return BadRequest(String.Format(ValidationMessages.CivilInstitution_Duplicate, CivilInstitution.InstitutionName));
			return BadRequest(ModelState);
		#endregion

		int insertedCivilInstitutionId = await _CivilInstitutionRepository.InsertCivilInstitution(CivilInstitution, logModel);
		return Created(nameof(GetCivilInstitutionById), new { id = insertedCivilInstitutionId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCivilInstitution(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CivilInstitutionModel CivilInstitution = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CivilInstitutionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CivilInstitution_InvalidId, id));
		if (CivilInstitution == null) return BadRequest(ValidationMessages.CivilInstitution_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CivilInstitution.CivilInstitutionId) return BadRequest(ValidationMessages.CivilInstitution_Mismatch);

		var CivilInstitutionToUpdate = await _CivilInstitutionRepository.GetCivilInstitutionById(id);
		if (CivilInstitutionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CivilInstitution_NotFoundId, id));
		#endregion

		await _CivilInstitutionRepository.UpdateCivilInstitution(CivilInstitution, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCivilInstitution(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CivilInstitution_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CivilInstitutionToDelete = await _CivilInstitutionRepository.GetCivilInstitutionById(id);
		if (CivilInstitutionToDelete == null)
			return NotFound(String.Format(ValidationMessages.CivilInstitution_NotFoundId, id));
		#endregion

		await _CivilInstitutionRepository.DeleteCivilInstitution(id, logModel);
		return NoContent(); // success
	});
}