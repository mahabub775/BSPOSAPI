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
public partial class CandidateCertificationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateCertificationController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateCertificationRepository _CandidateCertificationRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateCertificationController(ISecurityHelper securityHelper, ILogger<CandidateCertificationController> logger, IConfiguration config, ICandidateCertificationRepository CandidateCertificationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateCertificationRepository = CandidateCertificationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCertificationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateCertification_InvalidId, id));
		#endregion

		var result = await _CandidateCertificationRepository.GetCandidateCertificationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateCertification_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateCertificationsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCertificationsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateCertificationRepository.GetCandidateCertificationsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateCertification_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateCertification([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCertificationModel CandidateCertification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCertificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateCertification.CertificateAuthorityName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateCertification == null) return BadRequest(ValidationMessages.CandidateCertification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateCertification = await _CandidateCertificationRepository.GetCandidateCertificationByName(CandidateCertification.CourseName);
		//if (existingCandidateCertification != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateCertification_Duplicate, CandidateCertification.CourseName));
		#endregion

		int insertedCandidateCertificationId = await _CandidateCertificationRepository.InsertCandidateCertification(CandidateCertification, logModel);
		return Created(nameof(GetCandidateCertificationById), new { id = insertedCandidateCertificationId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateCertification(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCertificationModel CandidateCertification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCertificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCertification_InvalidId, id));
		if (CandidateCertification == null) return BadRequest(ValidationMessages.CandidateCertification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateCertification.CandidateCertificationId) return BadRequest(ValidationMessages.CandidateCertification_Mismatch);

		var CandidateCertificationToUpdate = await _CandidateCertificationRepository.GetCandidateCertificationById(id);
		if (CandidateCertificationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateCertification_NotFoundId, id));
		#endregion

		await _CandidateCertificationRepository.UpdateCandidateCertification(CandidateCertification, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateCertification(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCertification_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateCertificationToDelete = await _CandidateCertificationRepository.GetCandidateCertificationById(id);
		if (CandidateCertificationToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateCertification_NotFoundId, id));

		#endregion

		await _CandidateCertificationRepository.DeleteCandidateCertification(id, logModel);
		return NoContent(); // success
	});
}