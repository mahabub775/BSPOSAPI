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
public partial class CandidateAddlQualificationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateAddlQualificationController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateAddlQualificationRepository _CandidateAddlQualificationRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateAddlQualificationController(ISecurityHelper securityHelper, ILogger<CandidateAddlQualificationController> logger, IConfiguration config, ICandidateAddlQualificationRepository CandidateAddlQualificationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateAddlQualificationRepository = CandidateAddlQualificationRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateAddlQualificationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateAddlQualification_InvalidId, id));
		#endregion

		var result = await _CandidateAddlQualificationRepository.GetCandidateAddlQualificationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateAddlQualification_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateAddlQualificationsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateAddlQualificationsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateAddlQualificationRepository.GetCandidateAddlQualificationsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateAddlQualification_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateAddlQualification([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateAddlQualificationModel CandidateAddlQualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateAddlQualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateAddlQualification.QualificationName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateAddlQualification == null) return BadRequest(ValidationMessages.CandidateAddlQualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateAddlQualification = await _CandidateAddlQualificationRepository.GetCandidateAddlQualificationByName(CandidateAddlQualification.CourseName);
		//if (existingCandidateAddlQualification != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateAddlQualification_Duplicate, CandidateAddlQualification.CourseName));
		#endregion

		int insertedCandidateAddlQualificationId = await _CandidateAddlQualificationRepository.InsertCandidateAddlQualification(CandidateAddlQualification, logModel);
		return Created(nameof(GetCandidateAddlQualificationById), new { id = insertedCandidateAddlQualificationId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateAddlQualification(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateAddlQualificationModel CandidateAddlQualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateAddlQualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateAddlQualification_InvalidId, id));
		if (CandidateAddlQualification == null) return BadRequest(ValidationMessages.CandidateAddlQualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateAddlQualification.CandidateAddlQualificationId) return BadRequest(ValidationMessages.CandidateAddlQualification_Mismatch);

		var CandidateAddlQualificationToUpdate = await _CandidateAddlQualificationRepository.GetCandidateAddlQualificationById(id);
		if (CandidateAddlQualificationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateAddlQualification_NotFoundId, id));
		#endregion

		await _CandidateAddlQualificationRepository.UpdateCandidateAddlQualification(CandidateAddlQualification, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateAddlQualification(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateAddlQualification_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateAddlQualificationToDelete = await _CandidateAddlQualificationRepository.GetCandidateAddlQualificationById(id);
		if (CandidateAddlQualificationToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateAddlQualification_NotFoundId, id));

		#endregion

		await _CandidateAddlQualificationRepository.DeleteCandidateAddlQualification(id, logModel);
		return NoContent(); // success
	});
}