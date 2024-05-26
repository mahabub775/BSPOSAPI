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
public partial class ApplicantCivilEducationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantCivilEducationController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantCivilEducationRepository _ApplicantCivilEducationRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantCivilEducationController(ISecurityHelper securityHelper, ILogger<ApplicantCivilEducationController> logger, IConfiguration config, IApplicantCivilEducationRepository ApplicantCivilEducationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantCivilEducationRepository = ApplicantCivilEducationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCivilEducationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantCivilEducation_InvalidId, id));
		#endregion

		var result = await _ApplicantCivilEducationRepository.GetApplicantCivilEducationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCivilEducation_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantCivilEducationsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCivilEducationsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantCivilEducationRepository.GetApplicantCivilEducationsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantCivilEducation_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantCivilEducation([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCivilEducationModel ApplicantCivilEducation = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCivilEducationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantCivilEducation.DegreeName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantCivilEducation == null) return BadRequest(ValidationMessages.ApplicantCivilEducation_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantCivilEducation = await _ApplicantCivilEducationRepository.GetApplicantCivilEducationByName(ApplicantCivilEducation.CourseName);
		//if (existingApplicantCivilEducation != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantCivilEducation_Duplicate, ApplicantCivilEducation.CourseName));
		#endregion

		int insertedApplicantCivilEducationId = await _ApplicantCivilEducationRepository.InsertApplicantCivilEducation(ApplicantCivilEducation, logModel);
		return Created(nameof(GetApplicantCivilEducationById), new { id = insertedApplicantCivilEducationId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantCivilEducation(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCivilEducationModel ApplicantCivilEducation = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCivilEducationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCivilEducation_InvalidId, id));
		if (ApplicantCivilEducation == null) return BadRequest(ValidationMessages.ApplicantCivilEducation_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantCivilEducation.ApplicantCivilEducationId) return BadRequest(ValidationMessages.ApplicantCivilEducation_Mismatch);

		var ApplicantCivilEducationToUpdate = await _ApplicantCivilEducationRepository.GetApplicantCivilEducationById(id);
		if (ApplicantCivilEducationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCivilEducation_NotFoundId, id));
		#endregion

		await _ApplicantCivilEducationRepository.UpdateApplicantCivilEducation(ApplicantCivilEducation, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantCivilEducation(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCivilEducation_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantCivilEducationToDelete = await _ApplicantCivilEducationRepository.GetApplicantCivilEducationById(id);
		if (ApplicantCivilEducationToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCivilEducation_NotFoundId, id));

		#endregion

		await _ApplicantCivilEducationRepository.DeleteApplicantCivilEducation(id, logModel);
		return NoContent(); // success
	});
}