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
public partial class CandidateAssultCourseController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateAssultCourseController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateAssultCourseRepository _CandidateAssultCourseRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateAssultCourseController(ISecurityHelper securityHelper, ILogger<CandidateAssultCourseController> logger, IConfiguration config, ICandidateAssultCourseRepository CandidateAssultCourseRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateAssultCourseRepository = CandidateAssultCourseRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateAssultCourseById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateAssultCourse_InvalidId, id));
		#endregion

		var result = await _CandidateAssultCourseRepository.GetCandidateAssultCourseById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateAssultCourse_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateAssultCoursesByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateAssultCoursesByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateAssultCourseRepository.GetCandidateAssultCoursesByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateAssultCourse_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateAssultCourse([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateAssultCourseModel CandidateAssultCourse = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateAssultCourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateAssultCourse.Remarks))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateAssultCourse == null) return BadRequest(ValidationMessages.CandidateAssultCourse_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateAssultCourse = await _CandidateAssultCourseRepository.GetCandidateAssultCourseByName(CandidateAssultCourse.CourseName);
		//if (existingCandidateAssultCourse != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateAssultCourse_Duplicate, CandidateAssultCourse.CourseName));
		#endregion

		int insertedCandidateAssultCourseId = await _CandidateAssultCourseRepository.InsertCandidateAssultCourse(CandidateAssultCourse, logModel);
		return Created(nameof(GetCandidateAssultCourseById), new { id = insertedCandidateAssultCourseId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateAssultCourse(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateAssultCourseModel CandidateAssultCourse = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateAssultCourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateAssultCourse_InvalidId, id));
		if (CandidateAssultCourse == null) return BadRequest(ValidationMessages.CandidateAssultCourse_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateAssultCourse.CandidateAssultCourseId) return BadRequest(ValidationMessages.CandidateAssultCourse_Mismatch);

		var CandidateAssultCourseToUpdate = await _CandidateAssultCourseRepository.GetCandidateAssultCourseById(id);
		if (CandidateAssultCourseToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateAssultCourse_NotFoundId, id));
		#endregion

		await _CandidateAssultCourseRepository.UpdateCandidateAssultCourse(CandidateAssultCourse, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateAssultCourse(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateAssultCourse_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateAssultCourseToDelete = await _CandidateAssultCourseRepository.GetCandidateAssultCourseById(id);
		if (CandidateAssultCourseToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateAssultCourse_NotFoundId, id));

		#endregion

		await _CandidateAssultCourseRepository.DeleteCandidateAssultCourse(id, logModel);
		return NoContent(); // success
	});
}