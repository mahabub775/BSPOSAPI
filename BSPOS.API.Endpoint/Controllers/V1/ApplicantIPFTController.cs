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
public partial class ApplicantIPFTController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantIPFTController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantIPFTRepository _ApplicantIPFTRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantIPFTController(ISecurityHelper securityHelper, ILogger<ApplicantIPFTController> logger, IConfiguration config, IApplicantIPFTRepository ApplicantIPFTRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantIPFTRepository = ApplicantIPFTRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantIPFTById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantIPFT_InvalidId, id));
		#endregion

		var result = await _ApplicantIPFTRepository.GetApplicantIPFTById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantIPFT_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantIPFTsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantIPFTsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantIPFTRepository.GetApplicantIPFTsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantIPFT_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantIPFT([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantIPFTModel ApplicantIPFT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantIPFTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantIPFT.BIAnnualName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantIPFT == null) return BadRequest(ValidationMessages.ApplicantIPFT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantIPFT = await _ApplicantIPFTRepository.GetApplicantIPFTByName(ApplicantIPFT.CourseName);
		//if (existingApplicantIPFT != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantIPFT_Duplicate, ApplicantIPFT.CourseName));
		#endregion

		int insertedApplicantIPFTId = await _ApplicantIPFTRepository.InsertApplicantIPFT(ApplicantIPFT, logModel);
		return Created(nameof(GetApplicantIPFTById), new { id = insertedApplicantIPFTId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantIPFT(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantIPFTModel ApplicantIPFT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantIPFTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantIPFT_InvalidId, id));
		if (ApplicantIPFT == null) return BadRequest(ValidationMessages.ApplicantIPFT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantIPFT.ApplicantIPFTId) return BadRequest(ValidationMessages.ApplicantIPFT_Mismatch);

		var ApplicantIPFTToUpdate = await _ApplicantIPFTRepository.GetApplicantIPFTById(id);
		if (ApplicantIPFTToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantIPFT_NotFoundId, id));
		#endregion

		await _ApplicantIPFTRepository.UpdateApplicantIPFT(ApplicantIPFT, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantIPFT(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantIPFT_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantIPFTToDelete = await _ApplicantIPFTRepository.GetApplicantIPFTById(id);
		if (ApplicantIPFTToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantIPFT_NotFoundId, id));

		#endregion

		await _ApplicantIPFTRepository.DeleteApplicantIPFT(id, logModel);
		return NoContent(); // success
	});
}