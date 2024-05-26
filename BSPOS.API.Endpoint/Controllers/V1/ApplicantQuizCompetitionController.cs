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
public partial class ApplicantQuizCompetitionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantQuizCompetitionController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantQuizCompetitionRepository _ApplicantQuizCompetitionRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantQuizCompetitionController(ISecurityHelper securityHelper, ILogger<ApplicantQuizCompetitionController> logger, IConfiguration config, IApplicantQuizCompetitionRepository ApplicantQuizCompetitionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantQuizCompetitionRepository = ApplicantQuizCompetitionRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantQuizCompetitionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantQuizCompetition_InvalidId, id));
		#endregion

		var result = await _ApplicantQuizCompetitionRepository.GetApplicantQuizCompetitionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantQuizCompetition_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantQuizCompetitionsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantQuizCompetitionsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantQuizCompetitionRepository.GetApplicantQuizCompetitionsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantQuizCompetition_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantQuizCompetition([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantQuizCompetitionModel ApplicantQuizCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantQuizCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantQuizCompetition.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantQuizCompetition == null) return BadRequest(ValidationMessages.ApplicantQuizCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantQuizCompetition = await _ApplicantQuizCompetitionRepository.GetApplicantQuizCompetitionByName(ApplicantQuizCompetition.CourseName);
		//if (existingApplicantQuizCompetition != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantQuizCompetition_Duplicate, ApplicantQuizCompetition.CourseName));
		#endregion

		int insertedApplicantQuizCompetitionId = await _ApplicantQuizCompetitionRepository.InsertApplicantQuizCompetition(ApplicantQuizCompetition, logModel);
		return Created(nameof(GetApplicantQuizCompetitionById), new { id = insertedApplicantQuizCompetitionId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantQuizCompetition(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantQuizCompetitionModel ApplicantQuizCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantQuizCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantQuizCompetition_InvalidId, id));
		if (ApplicantQuizCompetition == null) return BadRequest(ValidationMessages.ApplicantQuizCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantQuizCompetition.ApplicantQuizCompetitionId) return BadRequest(ValidationMessages.ApplicantQuizCompetition_Mismatch);

		var ApplicantQuizCompetitionToUpdate = await _ApplicantQuizCompetitionRepository.GetApplicantQuizCompetitionById(id);
		if (ApplicantQuizCompetitionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantQuizCompetition_NotFoundId, id));
		#endregion

		await _ApplicantQuizCompetitionRepository.UpdateApplicantQuizCompetition(ApplicantQuizCompetition, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantQuizCompetition(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantQuizCompetition_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantQuizCompetitionToDelete = await _ApplicantQuizCompetitionRepository.GetApplicantQuizCompetitionById(id);
		if (ApplicantQuizCompetitionToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantQuizCompetition_NotFoundId, id));

		#endregion

		await _ApplicantQuizCompetitionRepository.DeleteApplicantQuizCompetition(id, logModel);
		return NoContent(); // success
	});
}