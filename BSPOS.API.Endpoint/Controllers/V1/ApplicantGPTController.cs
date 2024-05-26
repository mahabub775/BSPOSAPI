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
public partial class ApplicantGPTController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantGPTController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantGPTRepository _ApplicantGPTRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantGPTController(ISecurityHelper securityHelper, ILogger<ApplicantGPTController> logger, IConfiguration config, IApplicantGPTRepository ApplicantGPTRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantGPTRepository = ApplicantGPTRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantGPTById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantGPT_InvalidId, id));
		#endregion

		var result = await _ApplicantGPTRepository.GetApplicantGPTById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantGPT_NotFoundId, id));

		return Ok(result);
	});


	[HttpGet("GetTopPerformers"), AllowAnonymous]
	public Task<IActionResult> GetTopPerformers() =>
TryCatch(async () =>
{
	#region Validation
	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	{
		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
			return Unauthorized(ValidationMessages.InvalidHash);
	}
	#endregion

	var result = await _ApplicantGPTRepository.GetTopPerformers();
	if (result == null)
		return NotFound(ValidationMessages.ApplicantGPT_NotFoundList);

	return Ok(result);
});



	[HttpGet("GetApplicantGPTsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantGPTsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantGPTRepository.GetApplicantGPTsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantGPT_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantGPT([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantGPTModel ApplicantGPT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantGPTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantGPT.CreatedByName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantGPT == null) return BadRequest(ValidationMessages.ApplicantGPT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantGPT = await _ApplicantGPTRepository.GetApplicantGPTByName(ApplicantGPT.CourseName);
		//if (existingApplicantGPT != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantGPT_Duplicate, ApplicantGPT.CourseName));
		#endregion

		int insertedApplicantGPTId = await _ApplicantGPTRepository.InsertApplicantGPT(ApplicantGPT, logModel);
		return Created(nameof(GetApplicantGPTById), new { id = insertedApplicantGPTId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantGPT(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantGPTModel ApplicantGPT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantGPTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantGPT_InvalidId, id));
		if (ApplicantGPT == null) return BadRequest(ValidationMessages.ApplicantGPT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantGPT.ApplicantGPTId) return BadRequest(ValidationMessages.ApplicantGPT_Mismatch);

		var ApplicantGPTToUpdate = await _ApplicantGPTRepository.GetApplicantGPTById(id);
		if (ApplicantGPTToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantGPT_NotFoundId, id));
		#endregion

		await _ApplicantGPTRepository.UpdateApplicantGPT(ApplicantGPT, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantGPT(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantGPT_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantGPTToDelete = await _ApplicantGPTRepository.GetApplicantGPTById(id);
		if (ApplicantGPTToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantGPT_NotFoundId, id));

		#endregion

		await _ApplicantGPTRepository.DeleteApplicantGPT(id, logModel);
		return NoContent(); // success
	});
}