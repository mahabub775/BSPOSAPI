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
public partial class MilitaryInstitutionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<MilitaryInstitutionController> _logger;
	private readonly IConfiguration _config;
	private readonly IMilitaryInstitutionRepository _MilitaryInstitutionRepository;
	private readonly ICsvExporter _csvExporter;

	public MilitaryInstitutionController(ISecurityHelper securityHelper, ILogger<MilitaryInstitutionController> logger, IConfiguration config, IMilitaryInstitutionRepository MilitaryInstitutionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._MilitaryInstitutionRepository = MilitaryInstitutionRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetMilitaryInstitutions(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.MilitaryInstitution_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _MilitaryInstitutionRepository.GetMilitaryInstitutions(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.MilitaryInstitution_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctMilitaryInstitutions"), AllowAnonymous]
	public Task<IActionResult> GetDistinctMilitaryInstitutions() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _MilitaryInstitutionRepository.GetDistinctMilitaryInstitutions();
		if (result == null)
			return NotFound(ValidationMessages.MilitaryInstitution_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetMilitaryInstitutionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.MilitaryInstitution_InvalidId, id));
		#endregion

		var result = await _MilitaryInstitutionRepository.GetMilitaryInstitutionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.MilitaryInstitution_NotFoundId, id));

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

		var result = await _MilitaryInstitutionRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.MilitaryInstitution_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertMilitaryInstitution([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		MilitaryInstitutionModel MilitaryInstitution = PostData["Data"] == null ? null : JsonSerializer.Deserialize<MilitaryInstitutionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), MilitaryInstitution.InstitutionName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (MilitaryInstitution == null) return BadRequest(ValidationMessages.MilitaryInstitution_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingMilitaryInstitution = await _MilitaryInstitutionRepository.GetMilitaryInstitutionByName(MilitaryInstitution.InstitutionName);
		if (existingMilitaryInstitution != null)
			return BadRequest(String.Format(ValidationMessages.MilitaryInstitution_Duplicate, MilitaryInstitution.InstitutionName));
		#endregion

		int insertedMilitaryInstitutionId = await _MilitaryInstitutionRepository.InsertMilitaryInstitution(MilitaryInstitution, logModel);
		return Created(nameof(GetMilitaryInstitutionById), new { id = insertedMilitaryInstitutionId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateMilitaryInstitution(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		MilitaryInstitutionModel MilitaryInstitution = PostData["Data"] == null ? null : JsonSerializer.Deserialize<MilitaryInstitutionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.MilitaryInstitution_InvalidId, id));
		if (MilitaryInstitution == null) return BadRequest(ValidationMessages.MilitaryInstitution_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != MilitaryInstitution.MilitaryInstitutionId) return BadRequest(ValidationMessages.MilitaryInstitution_Mismatch);

		var MilitaryInstitutionToUpdate = await _MilitaryInstitutionRepository.GetMilitaryInstitutionById(id);
		if (MilitaryInstitutionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.MilitaryInstitution_NotFoundId, id));
		#endregion

		await _MilitaryInstitutionRepository.UpdateMilitaryInstitution(MilitaryInstitution, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteMilitaryInstitution(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.MilitaryInstitution_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var MilitaryInstitutionToDelete = await _MilitaryInstitutionRepository.GetMilitaryInstitutionById(id);
		if (MilitaryInstitutionToDelete == null)
			return NotFound(String.Format(ValidationMessages.MilitaryInstitution_NotFoundId, id));
		#endregion

		await _MilitaryInstitutionRepository.DeleteMilitaryInstitution(id, logModel);
		return NoContent(); // success
	});
}