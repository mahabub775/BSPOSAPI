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
public partial class CandidateRETController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateRETController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateRETRepository _CandidateRETRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateRETController(ISecurityHelper securityHelper, ILogger<CandidateRETController> logger, IConfiguration config, ICandidateRETRepository CandidateRETRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateRETRepository = CandidateRETRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateRETById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateRET_InvalidId, id));
		#endregion

		var result = await _CandidateRETRepository.GetCandidateRETById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateRET_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateRETsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateRETsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateRETRepository.GetCandidateRETsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateRET_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateRET([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateRETModel CandidateRET = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateRETModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateRET.BIAnnualName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateRET == null) return BadRequest(ValidationMessages.CandidateRET_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateRET = await _CandidateRETRepository.GetCandidateRETByName(CandidateRET.CourseName);
		//if (existingCandidateRET != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateRET_Duplicate, CandidateRET.CourseName));
		#endregion

		int insertedCandidateRETId = await _CandidateRETRepository.InsertCandidateRET(CandidateRET, logModel);
		return Created(nameof(GetCandidateRETById), new { id = insertedCandidateRETId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateRET(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateRETModel CandidateRET = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateRETModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateRET_InvalidId, id));
		if (CandidateRET == null) return BadRequest(ValidationMessages.CandidateRET_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateRET.CandidateRETId) return BadRequest(ValidationMessages.CandidateRET_Mismatch);

		var CandidateRETToUpdate = await _CandidateRETRepository.GetCandidateRETById(id);
		if (CandidateRETToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateRET_NotFoundId, id));
		#endregion

		await _CandidateRETRepository.UpdateCandidateRET(CandidateRET, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateRET(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateRET_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateRETToDelete = await _CandidateRETRepository.GetCandidateRETById(id);
		if (CandidateRETToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateRET_NotFoundId, id));

		#endregion

		await _CandidateRETRepository.DeleteCandidateRET(id, logModel);
		return NoContent(); // success
	});
}