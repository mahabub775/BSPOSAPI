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
public partial class ApplicantCompetitionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantCompetitionController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantCompetitionRepository _ApplicantCompetitionRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantCompetitionController(ISecurityHelper securityHelper, ILogger<ApplicantCompetitionController> logger, IConfiguration config, IApplicantCompetitionRepository ApplicantCompetitionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantCompetitionRepository = ApplicantCompetitionRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCompetitionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantCompetition_InvalidId, id));
		#endregion

		var result = await _ApplicantCompetitionRepository.GetApplicantCompetitionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCompetition_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantCompetitionsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCompetitionsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantCompetitionRepository.GetApplicantCompetitionsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantCompetition_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantCompetition([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCompetitionModel ApplicantCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantCompetition.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantCompetition == null) return BadRequest(ValidationMessages.ApplicantCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantCompetition = await _ApplicantCompetitionRepository.GetApplicantCompetitionByName(ApplicantCompetition.CourseName);
		//if (existingApplicantCompetition != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantCompetition_Duplicate, ApplicantCompetition.CourseName));
		#endregion

		int insertedApplicantCompetitionId = await _ApplicantCompetitionRepository.InsertApplicantCompetition(ApplicantCompetition, logModel);
		return Created(nameof(GetApplicantCompetitionById), new { id = insertedApplicantCompetitionId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantCompetition(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCompetitionModel ApplicantCompetition = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCompetitionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCompetition_InvalidId, id));
		if (ApplicantCompetition == null) return BadRequest(ValidationMessages.ApplicantCompetition_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantCompetition.ApplicantCompetitionId) return BadRequest(ValidationMessages.ApplicantCompetition_Mismatch);

		var ApplicantCompetitionToUpdate = await _ApplicantCompetitionRepository.GetApplicantCompetitionById(id);
		if (ApplicantCompetitionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCompetition_NotFoundId, id));
		#endregion

		await _ApplicantCompetitionRepository.UpdateApplicantCompetition(ApplicantCompetition, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantCompetition(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCompetition_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantCompetitionToDelete = await _ApplicantCompetitionRepository.GetApplicantCompetitionById(id);
		if (ApplicantCompetitionToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCompetition_NotFoundId, id));

		#endregion

		await _ApplicantCompetitionRepository.DeleteApplicantCompetition(id, logModel);
		return NoContent(); // success
	});
}