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
public partial class CandidateMilitaryTrainingController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateMilitaryTrainingController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateMilitaryTrainingRepository _CandidateMilitaryTrainingRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateMilitaryTrainingController(ISecurityHelper securityHelper, ILogger<CandidateMilitaryTrainingController> logger, IConfiguration config, ICandidateMilitaryTrainingRepository CandidateMilitaryTrainingRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateMilitaryTrainingRepository = CandidateMilitaryTrainingRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateMilitaryTrainingById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateMilitaryTraining_InvalidId, id));
		#endregion

		var result = await _CandidateMilitaryTrainingRepository.GetCandidateMilitaryTrainingById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateMilitaryTraining_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateMilitaryTrainingsByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateMilitaryTrainingsByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateMilitaryTrainingRepository.GetCandidateMilitaryTrainingsByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateMilitaryTraining_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateMilitaryTraining([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateMilitaryTrainingModel CandidateMilitaryTraining = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateMilitaryTrainingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateMilitaryTraining.TrainingName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateMilitaryTraining == null) return BadRequest(ValidationMessages.CandidateMilitaryTraining_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateMilitaryTraining = await _CandidateMilitaryTrainingRepository.GetCandidateMilitaryTrainingByName(CandidateMilitaryTraining.CourseName);
		//if (existingCandidateMilitaryTraining != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateMilitaryTraining_Duplicate, CandidateMilitaryTraining.CourseName));
		#endregion

		int insertedCandidateMilitaryTrainingId = await _CandidateMilitaryTrainingRepository.InsertCandidateMilitaryTraining(CandidateMilitaryTraining, logModel);
		return Created(nameof(GetCandidateMilitaryTrainingById), new { id = insertedCandidateMilitaryTrainingId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateMilitaryTraining(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateMilitaryTrainingModel CandidateMilitaryTraining = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateMilitaryTrainingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateMilitaryTraining_InvalidId, id));
		if (CandidateMilitaryTraining == null) return BadRequest(ValidationMessages.CandidateMilitaryTraining_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateMilitaryTraining.CandidateMilitaryTrainingId) return BadRequest(ValidationMessages.CandidateMilitaryTraining_Mismatch);

		var CandidateMilitaryTrainingToUpdate = await _CandidateMilitaryTrainingRepository.GetCandidateMilitaryTrainingById(id);
		if (CandidateMilitaryTrainingToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateMilitaryTraining_NotFoundId, id));
		#endregion

		await _CandidateMilitaryTrainingRepository.UpdateCandidateMilitaryTraining(CandidateMilitaryTraining, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateMilitaryTraining(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateMilitaryTraining_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateMilitaryTrainingToDelete = await _CandidateMilitaryTrainingRepository.GetCandidateMilitaryTrainingById(id);
		if (CandidateMilitaryTrainingToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateMilitaryTraining_NotFoundId, id));

		#endregion

		await _CandidateMilitaryTrainingRepository.DeleteCandidateMilitaryTraining(id, logModel);
		return NoContent(); // success
	});
}