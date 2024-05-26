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
public partial class QualificationController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<QualificationController> _logger;
	private readonly IConfiguration _config;
	private readonly IQualificationRepository _QualificationRepository;
	private readonly ICsvExporter _csvExporter;

	public QualificationController(ISecurityHelper securityHelper, ILogger<QualificationController> logger, IConfiguration config, IQualificationRepository QualificationRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._QualificationRepository = QualificationRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetQualifications(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Qualification_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _QualificationRepository.GetQualifications(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Qualification_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctQualifications"), AllowAnonymous]
	public Task<IActionResult> GetDistinctQualifications() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _QualificationRepository.GetDistinctQualifications();
		if (result == null)
			return NotFound(ValidationMessages.Qualification_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetQualificationById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Qualification_InvalidId, id));
		#endregion

		var result = await _QualificationRepository.GetQualificationById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Qualification_NotFoundId, id));

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

		var result = await _QualificationRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Qualification_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertQualification([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		QualificationModel Qualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<QualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Qualification.QualificationName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Qualification == null) return BadRequest(ValidationMessages.Qualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingQualification = await _QualificationRepository.GetQualificationByName(Qualification.QualificationName);
		if (existingQualification != null)
			return BadRequest(String.Format(ValidationMessages.Qualification_Duplicate, Qualification.QualificationName));
		#endregion

		int insertedQualificationId = await _QualificationRepository.InsertQualification(Qualification, logModel);
		return Created(nameof(GetQualificationById), new { id = insertedQualificationId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateQualification(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		QualificationModel Qualification = PostData["Data"] == null ? null : JsonSerializer.Deserialize<QualificationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Qualification_InvalidId, id));
		if (Qualification == null) return BadRequest(ValidationMessages.Qualification_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Qualification.QualificationId) return BadRequest(ValidationMessages.Qualification_Mismatch);

		var QualificationToUpdate = await _QualificationRepository.GetQualificationById(id);
		if (QualificationToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Qualification_NotFoundId, id));
		#endregion

		await _QualificationRepository.UpdateQualification(Qualification, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteQualification(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Qualification_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var QualificationToDelete = await _QualificationRepository.GetQualificationById(id);
		if (QualificationToDelete == null)
			return NotFound(String.Format(ValidationMessages.Qualification_NotFoundId, id));
		#endregion

		await _QualificationRepository.DeleteQualification(id, logModel);
		return NoContent(); // success
	});
}