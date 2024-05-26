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
public partial class DegreeController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<DegreeController> _logger;
	private readonly IConfiguration _config;
	private readonly IDegreeRepository _DegreeRepository;
	private readonly ICsvExporter _csvExporter;

	public DegreeController(ISecurityHelper securityHelper, ILogger<DegreeController> logger, IConfiguration config, IDegreeRepository DegreeRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._DegreeRepository = DegreeRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetDegrees(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Degree_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _DegreeRepository.GetDegrees(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Degree_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctDegrees"), AllowAnonymous]
	public Task<IActionResult> GetDistinctDegrees() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _DegreeRepository.GetDistinctDegrees();
		if (result == null)
			return NotFound(ValidationMessages.Degree_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetDegreeById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Degree_InvalidId, id));
		#endregion

		var result = await _DegreeRepository.GetDegreeById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Degree_NotFoundId, id));

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

		var result = await _DegreeRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Degree_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertDegree([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DegreeModel Degree = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DegreeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Degree.DegreeName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Degree == null) return BadRequest(ValidationMessages.Degree_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingDegree = await _DegreeRepository.GetDegreeByName(Degree.DegreeName);
		if (existingDegree != null)
			return BadRequest(String.Format(ValidationMessages.Degree_Duplicate, Degree.DegreeName));
		#endregion

		int insertedDegreeId = await _DegreeRepository.InsertDegree(Degree, logModel);
		return Created(nameof(GetDegreeById), new { id = insertedDegreeId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateDegree(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DegreeModel Degree = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DegreeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Degree_InvalidId, id));
		if (Degree == null) return BadRequest(ValidationMessages.Degree_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Degree.DegreeId) return BadRequest(ValidationMessages.Degree_Mismatch);

		var DegreeToUpdate = await _DegreeRepository.GetDegreeById(id);
		if (DegreeToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Degree_NotFoundId, id));
		#endregion

		await _DegreeRepository.UpdateDegree(Degree, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteDegree(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Degree_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var DegreeToDelete = await _DegreeRepository.GetDegreeById(id);
		if (DegreeToDelete == null)
			return NotFound(String.Format(ValidationMessages.Degree_NotFoundId, id));
		#endregion

		await _DegreeRepository.DeleteDegree(id, logModel);
		return NoContent(); // success
	});
}