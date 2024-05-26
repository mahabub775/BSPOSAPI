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
public partial class ApplicantCadreController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantCadreController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantCadreRepository _ApplicantCadreRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantCadreController(ISecurityHelper securityHelper, ILogger<ApplicantCadreController> logger, IConfiguration config, IApplicantCadreRepository ApplicantCadreRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantCadreRepository = ApplicantCadreRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCadreById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantCadre_InvalidId, id));
		#endregion

		var result = await _ApplicantCadreRepository.GetApplicantCadreById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCadre_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetApplicantCadresByApplicantId"), AllowAnonymous]
	public Task<IActionResult> GetApplicantCadresByApplicantId(int ApplicantId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantCadreRepository.GetApplicantCadresByApplicantId(ApplicantId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantCadre_NotFoundList);

		return Ok(result);
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantCadre([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCadreModel ApplicantCadre = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCadreModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantCadre.CourseName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantCadre == null) return BadRequest(ValidationMessages.ApplicantCadre_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantCadre = await _ApplicantCadreRepository.GetApplicantCadreByName(ApplicantCadre.CourseName);
		//if (existingApplicantCadre != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantCadre_Duplicate, ApplicantCadre.CourseName));
		#endregion

		int insertedApplicantCadreId = await _ApplicantCadreRepository.InsertApplicantCadre(ApplicantCadre, logModel);
		return Created(nameof(GetApplicantCadreById), new { id = insertedApplicantCadreId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantCadre(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantCadreModel ApplicantCadre = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantCadreModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCadre_InvalidId, id));
		if (ApplicantCadre == null) return BadRequest(ValidationMessages.ApplicantCadre_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantCadre.ApplicantCadreId) return BadRequest(ValidationMessages.ApplicantCadre_Mismatch);

		var ApplicantCadreToUpdate = await _ApplicantCadreRepository.GetApplicantCadreById(id);
		if (ApplicantCadreToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCadre_NotFoundId, id));
		#endregion

		await _ApplicantCadreRepository.UpdateApplicantCadre(ApplicantCadre, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantCadre(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantCadre_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantCadreToDelete = await _ApplicantCadreRepository.GetApplicantCadreById(id);
		if (ApplicantCadreToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantCadre_NotFoundId, id));

		#endregion

		await _ApplicantCadreRepository.DeleteApplicantCadre(id, logModel);
		return NoContent(); // success
	});
}