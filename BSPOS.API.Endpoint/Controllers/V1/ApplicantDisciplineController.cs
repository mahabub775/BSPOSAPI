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
public partial class ApplicantDisciplineController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantDisciplineController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantDisciplineRepository _ApplicantDisciplineRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantDisciplineController(ISecurityHelper securityHelper, ILogger<ApplicantDisciplineController> logger, IConfiguration config, IApplicantDisciplineRepository ApplicantDisciplineRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantDisciplineRepository = ApplicantDisciplineRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantDisciplineById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantDiscipline_InvalidId, id));
		#endregion

		var result = await _ApplicantDisciplineRepository.GetApplicantDisciplineById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantDiscipline_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantDisciplinesByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantDisciplinesByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantDisciplineRepository.GetApplicantDisciplinesByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantDiscipline_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantDiscipline([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantDisciplineModel ApplicantDiscipline = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantDisciplineModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantDiscipline.BAASectionName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantDiscipline == null) return BadRequest(ValidationMessages.ApplicantDiscipline_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantDiscipline = await _ApplicantDisciplineRepository.GetApplicantDisciplineByName(ApplicantDiscipline.CourseName);
		//if (existingApplicantDiscipline != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantDiscipline_Duplicate, ApplicantDiscipline.CourseName));
		#endregion

		int insertedApplicantDisciplineId = await _ApplicantDisciplineRepository.InsertApplicantDiscipline(ApplicantDiscipline, logModel);
		return Created(nameof(GetApplicantDisciplineById), new { id = insertedApplicantDisciplineId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantDiscipline(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantDisciplineModel ApplicantDiscipline = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantDisciplineModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantDiscipline_InvalidId, id));
		if (ApplicantDiscipline == null) return BadRequest(ValidationMessages.ApplicantDiscipline_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantDiscipline.ApplicantDisciplineId) return BadRequest(ValidationMessages.ApplicantDiscipline_Mismatch);

		var ApplicantDisciplineToUpdate = await _ApplicantDisciplineRepository.GetApplicantDisciplineById(id);
		if (ApplicantDisciplineToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantDiscipline_NotFoundId, id));
		#endregion

		await _ApplicantDisciplineRepository.UpdateApplicantDiscipline(ApplicantDiscipline, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantDiscipline(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantDiscipline_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantDisciplineToDelete = await _ApplicantDisciplineRepository.GetApplicantDisciplineById(id);
		if (ApplicantDisciplineToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantDiscipline_NotFoundId, id));

		#endregion

		await _ApplicantDisciplineRepository.DeleteApplicantDiscipline(id, logModel);
		return NoContent(); // success
	});
}