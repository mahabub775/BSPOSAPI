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
public partial class CandidateQuizCompetitionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateQuizCompetitionController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateQuizCompetitionRepository _CandidateQuizCompetitionRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateQuizCompetitionController(ISecurityHelper securityHelper, ILogger<CandidateQuizCompetitionController> logger, IConfiguration config, ICandidateQuizCompetitionRepository CandidateQuizCompetitionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateQuizCompetitionRepository = CandidateQuizCompetitionRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateQuizCompetitionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateQuizCompetition_InvalidId, id));
		#endregion

		var result = await _CandidateQuizCompetitionRepository.GetCandidateQuizCompetitionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateQuizCompetition_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateQuizCompetitionsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateQuizCompetitionsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateQuizCompetitionRepository.GetCandidateQuizCompetitionsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateQuizCompetition_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateQuizCompetition([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateQuizCompetitionModel CandidateQuizCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateQuizCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateQuizCompetition.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateQuizCompetition == null) return BadRequest(ValidationMessages.CandidateQuizCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateQuizCompetition = await _CandidateQuizCompetitionRepository.GetCandidateQuizCompetitionByName(CandidateQuizCompetition.CourseName);
		//if (existingCandidateQuizCompetition != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateQuizCompetition_Duplicate, CandidateQuizCompetition.CourseName));
		#endregion

		int insertedCandidateQuizCompetitionId = await _CandidateQuizCompetitionRepository.InsertCandidateQuizCompetition(CandidateQuizCompetition, logModel);
		return Created(nameof(GetCandidateQuizCompetitionById), new { id = insertedCandidateQuizCompetitionId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateQuizCompetition(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateQuizCompetitionModel CandidateQuizCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateQuizCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateQuizCompetition_InvalidId, id));
		if (CandidateQuizCompetition == null) return BadRequest(ValidationMessages.CandidateQuizCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateQuizCompetition.CandidateQuizCompetitionId) return BadRequest(ValidationMessages.CandidateQuizCompetition_Mismatch);

		var CandidateQuizCompetitionToUpdate = await _CandidateQuizCompetitionRepository.GetCandidateQuizCompetitionById(id);
		if (CandidateQuizCompetitionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateQuizCompetition_NotFoundId, id));
		#endregion

		await _CandidateQuizCompetitionRepository.UpdateCandidateQuizCompetition(CandidateQuizCompetition, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateQuizCompetition(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateQuizCompetition_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateQuizCompetitionToDelete = await _CandidateQuizCompetitionRepository.GetCandidateQuizCompetitionById(id);
		if (CandidateQuizCompetitionToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateQuizCompetition_NotFoundId, id));

		#endregion

		await _CandidateQuizCompetitionRepository.DeleteCandidateQuizCompetition(id, logModel);
		return NoContent(); // success
	});
}