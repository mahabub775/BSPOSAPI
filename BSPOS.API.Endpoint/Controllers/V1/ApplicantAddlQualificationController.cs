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
public partial class ApplicantAddlQualificationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantAddlQualificationController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantAddlQualificationRepository _ApplicantAddlQualificationRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantAddlQualificationController(ISecurityHelper securityHelper, ILogger<ApplicantAddlQualificationController> logger, IConfiguration config, IApplicantAddlQualificationRepository ApplicantAddlQualificationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantAddlQualificationRepository = ApplicantAddlQualificationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantAddlQualificationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantAddlQualification_InvalidId, id));
		#endregion

		var result = await _ApplicantAddlQualificationRepository.GetApplicantAddlQualificationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAddlQualification_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantAddlQualificationsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantAddlQualificationsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantAddlQualificationRepository.GetApplicantAddlQualificationsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantAddlQualification_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantAddlQualification([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantAddlQualificationModel ApplicantAddlQualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantAddlQualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantAddlQualification.QualificationName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantAddlQualification == null) return BadRequest(ValidationMessages.ApplicantAddlQualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantAddlQualification = await _ApplicantAddlQualificationRepository.GetApplicantAddlQualificationByName(ApplicantAddlQualification.CourseName);
		//if (existingApplicantAddlQualification != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantAddlQualification_Duplicate, ApplicantAddlQualification.CourseName));
		#endregion

		int insertedApplicantAddlQualificationId = await _ApplicantAddlQualificationRepository.InsertApplicantAddlQualification(ApplicantAddlQualification, logModel);
		return Created(nameof(GetApplicantAddlQualificationById), new { id = insertedApplicantAddlQualificationId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantAddlQualification(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantAddlQualificationModel ApplicantAddlQualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantAddlQualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantAddlQualification_InvalidId, id));
		if (ApplicantAddlQualification == null) return BadRequest(ValidationMessages.ApplicantAddlQualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantAddlQualification.ApplicantAddlQualificationId) return BadRequest(ValidationMessages.ApplicantAddlQualification_Mismatch);

		var ApplicantAddlQualificationToUpdate = await _ApplicantAddlQualificationRepository.GetApplicantAddlQualificationById(id);
		if (ApplicantAddlQualificationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAddlQualification_NotFoundId, id));
		#endregion

		await _ApplicantAddlQualificationRepository.UpdateApplicantAddlQualification(ApplicantAddlQualification, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantAddlQualification(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantAddlQualification_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantAddlQualificationToDelete = await _ApplicantAddlQualificationRepository.GetApplicantAddlQualificationById(id);
		if (ApplicantAddlQualificationToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAddlQualification_NotFoundId, id));

		#endregion

		await _ApplicantAddlQualificationRepository.DeleteApplicantAddlQualification(id, logModel);
		return NoContent(); // success
	});
}