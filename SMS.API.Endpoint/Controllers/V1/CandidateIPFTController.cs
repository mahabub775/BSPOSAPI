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
public partial class CandidateIPFTController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateIPFTController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateIPFTRepository _CandidateIPFTRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateIPFTController(ISecurityHelper securityHelper, ILogger<CandidateIPFTController> logger, IConfiguration config, ICandidateIPFTRepository CandidateIPFTRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateIPFTRepository = CandidateIPFTRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateIPFTById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateIPFT_InvalidId, id));
		#endregion

		var result = await _CandidateIPFTRepository.GetCandidateIPFTById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateIPFT_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateIPFTsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateIPFTsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateIPFTRepository.GetCandidateIPFTsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateIPFT_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateIPFT([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateIPFTModel CandidateIPFT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateIPFTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateIPFT.BIAnnualName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateIPFT == null) return BadRequest(ValidationMessages.CandidateIPFT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateIPFT = await _CandidateIPFTRepository.GetCandidateIPFTByName(CandidateIPFT.CourseName);
		//if (existingCandidateIPFT != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateIPFT_Duplicate, CandidateIPFT.CourseName));
		#endregion

		int insertedCandidateIPFTId = await _CandidateIPFTRepository.InsertCandidateIPFT(CandidateIPFT, logModel);
		return Created(nameof(GetCandidateIPFTById), new { id = insertedCandidateIPFTId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateIPFT(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateIPFTModel CandidateIPFT = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateIPFTModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateIPFT_InvalidId, id));
		if (CandidateIPFT == null) return BadRequest(ValidationMessages.CandidateIPFT_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateIPFT.CandidateIPFTId) return BadRequest(ValidationMessages.CandidateIPFT_Mismatch);

		var CandidateIPFTToUpdate = await _CandidateIPFTRepository.GetCandidateIPFTById(id);
		if (CandidateIPFTToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateIPFT_NotFoundId, id));
		#endregion

		await _CandidateIPFTRepository.UpdateCandidateIPFT(CandidateIPFT, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateIPFT(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateIPFT_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateIPFTToDelete = await _CandidateIPFTRepository.GetCandidateIPFTById(id);
		if (CandidateIPFTToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateIPFT_NotFoundId, id));

		#endregion

		await _CandidateIPFTRepository.DeleteCandidateIPFT(id, logModel);
		return NoContent(); // success
	});
}