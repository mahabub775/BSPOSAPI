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
public partial class ApplicantAssultCourseController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantAssultCourseController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantAssultCourseRepository _ApplicantAssultCourseRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantAssultCourseController(ISecurityHelper securityHelper, ILogger<ApplicantAssultCourseController> logger, IConfiguration config, IApplicantAssultCourseRepository ApplicantAssultCourseRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantAssultCourseRepository = ApplicantAssultCourseRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantAssultCourseById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantAssultCourse_InvalidId, id));
		#endregion

		var result = await _ApplicantAssultCourseRepository.GetApplicantAssultCourseById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAssultCourse_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantAssultCoursesByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantAssultCoursesByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantAssultCourseRepository.GetApplicantAssultCoursesByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantAssultCourse_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantAssultCourse([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantAssultCourseModel ApplicantAssultCourse = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantAssultCourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantAssultCourse.Remarks))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantAssultCourse == null) return BadRequest(ValidationMessages.ApplicantAssultCourse_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantAssultCourse = await _ApplicantAssultCourseRepository.GetApplicantAssultCourseByName(ApplicantAssultCourse.CourseName);
		//if (existingApplicantAssultCourse != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantAssultCourse_Duplicate, ApplicantAssultCourse.CourseName));
		#endregion

		int insertedApplicantAssultCourseId = await _ApplicantAssultCourseRepository.InsertApplicantAssultCourse(ApplicantAssultCourse, logModel);
		return Created(nameof(GetApplicantAssultCourseById), new { id = insertedApplicantAssultCourseId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantAssultCourse(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantAssultCourseModel ApplicantAssultCourse = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantAssultCourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantAssultCourse_InvalidId, id));
		if (ApplicantAssultCourse == null) return BadRequest(ValidationMessages.ApplicantAssultCourse_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantAssultCourse.ApplicantAssultCourseId) return BadRequest(ValidationMessages.ApplicantAssultCourse_Mismatch);

		var ApplicantAssultCourseToUpdate = await _ApplicantAssultCourseRepository.GetApplicantAssultCourseById(id);
		if (ApplicantAssultCourseToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAssultCourse_NotFoundId, id));
		#endregion

		await _ApplicantAssultCourseRepository.UpdateApplicantAssultCourse(ApplicantAssultCourse, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantAssultCourse(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantAssultCourse_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantAssultCourseToDelete = await _ApplicantAssultCourseRepository.GetApplicantAssultCourseById(id);
		if (ApplicantAssultCourseToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantAssultCourse_NotFoundId, id));

		#endregion

		await _ApplicantAssultCourseRepository.DeleteApplicantAssultCourse(id, logModel);
		return NoContent(); // success
	});
}