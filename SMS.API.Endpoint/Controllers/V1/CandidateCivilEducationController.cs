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
public partial class CandidateCivilEducationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateCivilEducationController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateCivilEducationRepository _CandidateCivilEducationRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateCivilEducationController(ISecurityHelper securityHelper, ILogger<CandidateCivilEducationController> logger, IConfiguration config, ICandidateCivilEducationRepository CandidateCivilEducationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateCivilEducationRepository = CandidateCivilEducationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCivilEducationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateCivilEducation_InvalidId, id));
		#endregion

		var result = await _CandidateCivilEducationRepository.GetCandidateCivilEducationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateCivilEducation_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateCivilEducationsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCivilEducationsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateCivilEducationRepository.GetCandidateCivilEducationsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateCivilEducation_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateCivilEducation([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCivilEducationModel CandidateCivilEducation = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCivilEducationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateCivilEducation.DegreeName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateCivilEducation == null) return BadRequest(ValidationMessages.CandidateCivilEducation_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateCivilEducation = await _CandidateCivilEducationRepository.GetCandidateCivilEducationByName(CandidateCivilEducation.CourseName);
		//if (existingCandidateCivilEducation != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateCivilEducation_Duplicate, CandidateCivilEducation.CourseName));
		#endregion

		int insertedCandidateCivilEducationId = await _CandidateCivilEducationRepository.InsertCandidateCivilEducation(CandidateCivilEducation, logModel);
		return Created(nameof(GetCandidateCivilEducationById), new { id = insertedCandidateCivilEducationId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateCivilEducation(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCivilEducationModel CandidateCivilEducation = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCivilEducationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCivilEducation_InvalidId, id));
		if (CandidateCivilEducation == null) return BadRequest(ValidationMessages.CandidateCivilEducation_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateCivilEducation.CandidateCivilEducationId) return BadRequest(ValidationMessages.CandidateCivilEducation_Mismatch);

		var CandidateCivilEducationToUpdate = await _CandidateCivilEducationRepository.GetCandidateCivilEducationById(id);
		if (CandidateCivilEducationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateCivilEducation_NotFoundId, id));
		#endregion

		await _CandidateCivilEducationRepository.UpdateCandidateCivilEducation(CandidateCivilEducation, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateCivilEducation(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCivilEducation_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateCivilEducationToDelete = await _CandidateCivilEducationRepository.GetCandidateCivilEducationById(id);
		if (CandidateCivilEducationToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateCivilEducation_NotFoundId, id));

		#endregion

		await _CandidateCivilEducationRepository.DeleteCandidateCivilEducation(id, logModel);
		return NoContent(); // success
	});
}