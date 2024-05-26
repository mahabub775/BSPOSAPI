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
public partial class CandidateCompetitionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateCompetitionController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateCompetitionRepository _CandidateCompetitionRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateCompetitionController(ISecurityHelper securityHelper, ILogger<CandidateCompetitionController> logger, IConfiguration config, ICandidateCompetitionRepository CandidateCompetitionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateCompetitionRepository = CandidateCompetitionRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCompetitionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateCompetition_InvalidId, id));
		#endregion

		var result = await _CandidateCompetitionRepository.GetCandidateCompetitionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateCompetition_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateCompetitionsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCompetitionsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateCompetitionRepository.GetCandidateCompetitionsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateCompetition_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateCompetition([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCompetitionModel CandidateCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateCompetition.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateCompetition == null) return BadRequest(ValidationMessages.CandidateCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateCompetition = await _CandidateCompetitionRepository.GetCandidateCompetitionByName(CandidateCompetition.CourseName);
		//if (existingCandidateCompetition != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateCompetition_Duplicate, CandidateCompetition.CourseName));
		#endregion

		int insertedCandidateCompetitionId = await _CandidateCompetitionRepository.InsertCandidateCompetition(CandidateCompetition, logModel);
		return Created(nameof(GetCandidateCompetitionById), new { id = insertedCandidateCompetitionId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateCompetition(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCompetitionModel CandidateCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCompetition_InvalidId, id));
		if (CandidateCompetition == null) return BadRequest(ValidationMessages.CandidateCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateCompetition.CandidateCompetitionId) return BadRequest(ValidationMessages.CandidateCompetition_Mismatch);

		var CandidateCompetitionToUpdate = await _CandidateCompetitionRepository.GetCandidateCompetitionById(id);
		if (CandidateCompetitionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateCompetition_NotFoundId, id));
		#endregion

		await _CandidateCompetitionRepository.UpdateCandidateCompetition(CandidateCompetition, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateCompetition(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCompetition_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateCompetitionToDelete = await _CandidateCompetitionRepository.GetCandidateCompetitionById(id);
		if (CandidateCompetitionToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateCompetition_NotFoundId, id));

		#endregion

		await _CandidateCompetitionRepository.DeleteCandidateCompetition(id, logModel);
		return NoContent(); // success
	});
}