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
public partial class CandidateCadreController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateCadreController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateCadreRepository _CandidateCadreRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateCadreController(ISecurityHelper securityHelper, ILogger<CandidateCadreController> logger, IConfiguration config, ICandidateCadreRepository CandidateCadreRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateCadreRepository = CandidateCadreRepository;
		this._csvExporter = csvExporter;
	}


	//this is get by function 

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCadreById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CandidateCadre_InvalidId, id));
		#endregion

		var result = await _CandidateCadreRepository.GetCandidateCadreById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CandidateCadre_NotFoundId, id));

		return Ok(result);
	});



	[HttpGet("GetCandidateCadresByCandidateId"), AllowAnonymous]
	public Task<IActionResult> GetCandidateCadresByCandidateId(int CandidateId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CandidateCadreRepository.GetCandidateCadresByCandidateId(CandidateId);
		if (result == null)
			return NotFound(ValidationMessages.CandidateCadre_NotFoundList);

		return Ok(result);
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidateCadre([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCadreModel CandidateCadre = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCadreModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CandidateCadre.CourseName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CandidateCadre == null) return BadRequest(ValidationMessages.CandidateCadre_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingCandidateCadre = await _CandidateCadreRepository.GetCandidateCadreByName(CandidateCadre.CourseName);
		//if (existingCandidateCadre != null)
		//	return BadRequest(String.Format(ValidationMessages.CandidateCadre_Duplicate, CandidateCadre.CourseName));
		#endregion

		int insertedCandidateCadreId = await _CandidateCadreRepository.InsertCandidateCadre(CandidateCadre, logModel);
		return Created(nameof(GetCandidateCadreById), new { id = insertedCandidateCadreId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidateCadre(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateCadreModel CandidateCadre = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateCadreModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCadre_InvalidId, id));
		if (CandidateCadre == null) return BadRequest(ValidationMessages.CandidateCadre_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CandidateCadre.CandidateCadreId) return BadRequest(ValidationMessages.CandidateCadre_Mismatch);

		var CandidateCadreToUpdate = await _CandidateCadreRepository.GetCandidateCadreById(id);
		if (CandidateCadreToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CandidateCadre_NotFoundId, id));
		#endregion

		await _CandidateCadreRepository.UpdateCandidateCadre(CandidateCadre, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCandidateCadre(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CandidateCadre_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CandidateCadreToDelete = await _CandidateCadreRepository.GetCandidateCadreById(id);
		if (CandidateCadreToDelete == null)
			return NotFound(String.Format(ValidationMessages.CandidateCadre_NotFoundId, id));

		#endregion

		await _CandidateCadreRepository.DeleteCandidateCadre(id, logModel);
		return NoContent(); // success
	});
}