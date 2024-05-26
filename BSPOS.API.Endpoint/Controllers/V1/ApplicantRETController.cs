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
public partial class ApplicantRETController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantRETController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantRETRepository _ApplicantRETRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantRETController(ISecurityHelper securityHelper, ILogger<ApplicantRETController> logger, IConfiguration config, IApplicantRETRepository ApplicantRETRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantRETRepository = ApplicantRETRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantRETById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantRET_InvalidId, id));
		#endregion

		var result = await _ApplicantRETRepository.GetApplicantRETById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRET_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantRETsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantRETsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantRETRepository.GetApplicantRETsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantRET_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantRET([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantRETModel ApplicantRET = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantRETModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantRET.BIAnnualName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantRET == null) return BadRequest(ValidationMessages.ApplicantRET_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantRET = await _ApplicantRETRepository.GetApplicantRETByName(ApplicantRET.CourseName);
		//if (existingApplicantRET != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantRET_Duplicate, ApplicantRET.CourseName));
		#endregion

		int insertedApplicantRETId = await _ApplicantRETRepository.InsertApplicantRET(ApplicantRET, logModel);
		return Created(nameof(GetApplicantRETById), new { id = insertedApplicantRETId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantRET(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantRETModel ApplicantRET = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantRETModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantRET_InvalidId, id));
		if (ApplicantRET == null) return BadRequest(ValidationMessages.ApplicantRET_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantRET.ApplicantRETId) return BadRequest(ValidationMessages.ApplicantRET_Mismatch);

		var ApplicantRETToUpdate = await _ApplicantRETRepository.GetApplicantRETById(id);
		if (ApplicantRETToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRET_NotFoundId, id));
		#endregion

		await _ApplicantRETRepository.UpdateApplicantRET(ApplicantRET, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantRET(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantRET_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantRETToDelete = await _ApplicantRETRepository.GetApplicantRETById(id);
		if (ApplicantRETToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRET_NotFoundId, id));

		#endregion

		await _ApplicantRETRepository.DeleteApplicantRET(id, logModel);
		return NoContent(); // success
	});
}