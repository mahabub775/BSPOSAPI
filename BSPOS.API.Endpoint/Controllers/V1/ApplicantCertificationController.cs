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
public partial class ApplicantCertificationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantCertificationController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantCertificationRepository _ApplicantCertificationRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantCertificationController(ISecurityHelper securityHelper, ILogger<ApplicantCertificationController> logger, IConfiguration config, IApplicantCertificationRepository ApplicantCertificationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantCertificationRepository = ApplicantCertificationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCertificationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantCertification_InvalidId, id));
		#endregion

		var result = await _ApplicantCertificationRepository.GetApplicantCertificationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCertification_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantCertificationsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCertificationsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantCertificationRepository.GetApplicantCertificationsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantCertification_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantCertification([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCertificationModel ApplicantCertification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCertificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantCertification.CertificateAuthorityName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantCertification == null) return BadRequest(ValidationMessages.ApplicantCertification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantCertification = await _ApplicantCertificationRepository.GetApplicantCertificationByName(ApplicantCertification.CourseName);
		//if (existingApplicantCertification != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantCertification_Duplicate, ApplicantCertification.CourseName));
		#endregion

		int insertedApplicantCertificationId = await _ApplicantCertificationRepository.InsertApplicantCertification(ApplicantCertification, logModel);
		return Created(nameof(GetApplicantCertificationById), new { id = insertedApplicantCertificationId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantCertification(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCertificationModel ApplicantCertification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCertificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCertification_InvalidId, id));
		if (ApplicantCertification == null) return BadRequest(ValidationMessages.ApplicantCertification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantCertification.ApplicantCertificationId) return BadRequest(ValidationMessages.ApplicantCertification_Mismatch);

		var ApplicantCertificationToUpdate = await _ApplicantCertificationRepository.GetApplicantCertificationById(id);
		if (ApplicantCertificationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCertification_NotFoundId, id));
		#endregion

		await _ApplicantCertificationRepository.UpdateApplicantCertification(ApplicantCertification, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantCertification(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCertification_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantCertificationToDelete = await _ApplicantCertificationRepository.GetApplicantCertificationById(id);
		if (ApplicantCertificationToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCertification_NotFoundId, id));

		#endregion

		await _ApplicantCertificationRepository.DeleteApplicantCertification(id, logModel);
		return NoContent(); // success
	});
}