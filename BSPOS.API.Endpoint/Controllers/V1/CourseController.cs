using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
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
public partial class CourseController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CourseController> _logger;
	private readonly IConfiguration _config;
	private readonly ICourseRepository _CourseRepository;
	private readonly ICsvExporter _csvExporter;

	public CourseController(ISecurityHelper securityHelper, ILogger<CourseController> logger, IConfiguration config, ICourseRepository CourseRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CourseRepository = CourseRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCourses(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Course_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CourseRepository.GetCourses(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Course_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCourses"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCourses() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CourseRepository.GetDistinctCourses();
		if (result == null)
			return NotFound(ValidationMessages.Course_NotFoundList);

		return Ok(result);
	});
	[HttpGet("GetTopMilitaryCourseTaken"), AllowAnonymous]
	public Task<IActionResult> GetTopMilitaryCourseTaken() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CourseRepository.GetTopMilitaryCourseTaken();
		if (result == null)
			return NotFound(ValidationMessages.Course_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCourseById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Course_InvalidId, id));
		#endregion

		var result = await _CourseRepository.GetCourseById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Course_NotFoundId, id));

		return Ok(result);
	});




	[HttpGet("Export"), AllowAnonymous]
	public Task<IActionResult> Export() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CourseRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Course_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertCourse([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CourseModel Course = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Course.CourseName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Course == null) return BadRequest(ValidationMessages.Course_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCourse = await _CourseRepository.GetCourseByName(Course.CourseName);
		if (existingCourse != null)
			return BadRequest(String.Format(ValidationMessages.Course_Duplicate, Course.CourseName));
			//return BadRequest(ModelState);
		#endregion

		int insertedCourseId = await _CourseRepository.InsertCourse(Course, logModel);
		return Created(nameof(GetCourseById), new { id = insertedCourseId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateCourse(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CourseModel Course = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CourseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Course_InvalidId, id));
		if (Course == null) return BadRequest(ValidationMessages.Course_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Course.CourseId) return BadRequest(ValidationMessages.Course_Mismatch);

		var CourseToUpdate = await _CourseRepository.GetCourseById(id);
		if (CourseToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Course_NotFoundId, id));
		#endregion

		await _CourseRepository.UpdateCourse(Course, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteCourse(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Course_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CourseToDelete = await _CourseRepository.GetCourseById(id);
		if (CourseToDelete == null)
			return NotFound(String.Format(ValidationMessages.Course_NotFoundId, id));
		#endregion

		await _CourseRepository.DeleteCourse(id, logModel);
		return NoContent(); // success
	});
}