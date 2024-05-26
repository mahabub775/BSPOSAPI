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
public partial class CandidateDisciplineController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateDisciplineController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateDisciplineRepository _CandidateDisciplineRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateDisciplineController(ISecurityHelper securityHelper, ILogger<CandidateDisciplineController> logger, IConfiguration config, ICandidateDisciplineRepository CandidateDisciplineRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateDisciplineRepository = CandidateDisciplineRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateDisciplineById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateDiscipline_InvalidId, id));
		#endregion

		var result = await _CandidateDisciplineRepository.GetCandidateDisciplineById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateDiscipline_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateDisciplinesByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateDisciplinesByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateDisciplineRepository.GetCandidateDisciplinesByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateDiscipline_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateDiscipline([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateDisciplineModel CandidateDiscipline = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateDisciplineModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateDiscipline.BAASectionName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateDiscipline == null) return BadRequest(ValidationMessages.CandidateDiscipline_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateDiscipline = await _CandidateDisciplineRepository.GetCandidateDisciplineByName(CandidateDiscipline.CourseName);
		//if (existingCandidateDiscipline != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateDiscipline_Duplicate, CandidateDiscipline.CourseName));
		#endregion

		int insertedCandidateDisciplineId = await _CandidateDisciplineRepository.InsertCandidateDiscipline(CandidateDiscipline, logModel);
		return Created(nameof(GetCandidateDisciplineById), new { id = insertedCandidateDisciplineId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateDiscipline(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateDisciplineModel CandidateDiscipline = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateDisciplineModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateDiscipline_InvalidId, id));
		if (CandidateDiscipline == null) return BadRequest(ValidationMessages.CandidateDiscipline_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateDiscipline.CandidateDisciplineId) return BadRequest(ValidationMessages.CandidateDiscipline_Mismatch);

		var CandidateDisciplineToUpdate = await _CandidateDisciplineRepository.GetCandidateDisciplineById(id);
		if (CandidateDisciplineToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateDiscipline_NotFoundId, id));
		#endregion

		await _CandidateDisciplineRepository.UpdateCandidateDiscipline(CandidateDiscipline, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateDiscipline(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateDiscipline_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateDisciplineToDelete = await _CandidateDisciplineRepository.GetCandidateDisciplineById(id);
		if (CandidateDisciplineToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateDiscipline_NotFoundId, id));

		#endregion

		await _CandidateDisciplineRepository.DeleteCandidateDiscipline(id, logModel);
		return NoContent(); // success
	});
}