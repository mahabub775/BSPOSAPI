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
public partial class CandidateGPTController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateGPTController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateGPTRepository _CandidateGPTRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateGPTController(ISecurityHelper securityHelper, ILogger<CandidateGPTController> logger, IConfiguration config, ICandidateGPTRepository CandidateGPTRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateGPTRepository = CandidateGPTRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateGPTById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateGPT_InvalidId, id));
		#endregion

		var result = await _CandidateGPTRepository.GetCandidateGPTById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateGPT_NotFoundId, id));

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

	var result = await _CandidateGPTRepository.GetTopPerformers();
	if (result == null)
		return NotFound(ValidationMessages.CandidateGPT_NotFoundList);

	return Ok(result);
});



	[HttpGet("GetCandidateGPTsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateGPTsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateGPTRepository.GetCandidateGPTsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateGPT_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateGPT([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateGPTModel CandidateGPT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateGPTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateGPT.CreatedByName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateGPT == null) return BadRequest(ValidationMessages.CandidateGPT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateGPT = await _CandidateGPTRepository.GetCandidateGPTByName(CandidateGPT.CourseName);
		//if (existingCandidateGPT != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateGPT_Duplicate, CandidateGPT.CourseName));
		#endregion

		int insertedCandidateGPTId = await _CandidateGPTRepository.InsertCandidateGPT(CandidateGPT, logModel);
		return Created(nameof(GetCandidateGPTById), new { id = insertedCandidateGPTId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateGPT(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateGPTModel CandidateGPT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateGPTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateGPT_InvalidId, id));
		if (CandidateGPT == null) return BadRequest(ValidationMessages.CandidateGPT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateGPT.CandidateGPTId) return BadRequest(ValidationMessages.CandidateGPT_Mismatch);

		var CandidateGPTToUpdate = await _CandidateGPTRepository.GetCandidateGPTById(id);
		if (CandidateGPTToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateGPT_NotFoundId, id));
		#endregion

		await _CandidateGPTRepository.UpdateCandidateGPT(CandidateGPT, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateGPT(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateGPT_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateGPTToDelete = await _CandidateGPTRepository.GetCandidateGPTById(id);
		if (CandidateGPTToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateGPT_NotFoundId, id));

		#endregion

		await _CandidateGPTRepository.DeleteCandidateGPT(id, logModel);
		return NoContent(); // success
	});
}