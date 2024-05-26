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
public partial class ApplicantMilitaryTrainingController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantMilitaryTrainingController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantMilitaryTrainingRepository _ApplicantMilitaryTrainingRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantMilitaryTrainingController(ISecurityHelper securityHelper, ILogger<ApplicantMilitaryTrainingController> logger, IConfiguration config, IApplicantMilitaryTrainingRepository ApplicantMilitaryTrainingRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantMilitaryTrainingRepository = ApplicantMilitaryTrainingRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantMilitaryTrainingById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantMilitaryTraining_InvalidId, id));
		#endregion

		var result = await _ApplicantMilitaryTrainingRepository.GetApplicantMilitaryTrainingById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantMilitaryTraining_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantMilitaryTrainingsByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantMilitaryTrainingsByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantMilitaryTrainingRepository.GetApplicantMilitaryTrainingsByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantMilitaryTraining_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantMilitaryTraining([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantMilitaryTrainingModel ApplicantMilitaryTraining = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantMilitaryTrainingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantMilitaryTraining.TrainingName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantMilitaryTraining == null) return BadRequest(ValidationMessages.ApplicantMilitaryTraining_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantMilitaryTraining = await _ApplicantMilitaryTrainingRepository.GetApplicantMilitaryTrainingByName(ApplicantMilitaryTraining.CourseName);
		//if (existingApplicantMilitaryTraining != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantMilitaryTraining_Duplicate, ApplicantMilitaryTraining.CourseName));
		#endregion

		int insertedApplicantMilitaryTrainingId = await _ApplicantMilitaryTrainingRepository.InsertApplicantMilitaryTraining(ApplicantMilitaryTraining, logModel);
		return Created(nameof(GetApplicantMilitaryTrainingById), new { id = insertedApplicantMilitaryTrainingId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantMilitaryTraining(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantMilitaryTrainingModel ApplicantMilitaryTraining = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantMilitaryTrainingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantMilitaryTraining_InvalidId, id));
		if (ApplicantMilitaryTraining == null) return BadRequest(ValidationMessages.ApplicantMilitaryTraining_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantMilitaryTraining.ApplicantMilitaryTrainingId) return BadRequest(ValidationMessages.ApplicantMilitaryTraining_Mismatch);

		var ApplicantMilitaryTrainingToUpdate = await _ApplicantMilitaryTrainingRepository.GetApplicantMilitaryTrainingById(id);
		if (ApplicantMilitaryTrainingToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantMilitaryTraining_NotFoundId, id));
		#endregion

		await _ApplicantMilitaryTrainingRepository.UpdateApplicantMilitaryTraining(ApplicantMilitaryTraining, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantMilitaryTraining(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantMilitaryTraining_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantMilitaryTrainingToDelete = await _ApplicantMilitaryTrainingRepository.GetApplicantMilitaryTrainingById(id);
		if (ApplicantMilitaryTrainingToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantMilitaryTraining_NotFoundId, id));

		#endregion

		await _ApplicantMilitaryTrainingRepository.DeleteApplicantMilitaryTraining(id, logModel);
		return NoContent(); // success
	});
}