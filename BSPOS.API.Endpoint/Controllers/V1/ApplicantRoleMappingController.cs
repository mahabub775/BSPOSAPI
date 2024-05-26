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
public partial class ApplicantRoleMappingController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantRoleMappingController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantRoleMappingRepository _ApplicantRoleMappingRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantRoleMappingController(ISecurityHelper securityHelper, ILogger<ApplicantRoleMappingController> logger, IConfiguration config, IApplicantRoleMappingRepository ApplicantRoleMappingRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantRoleMappingRepository = ApplicantRoleMappingRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet("GetApplicantRoleMappings/{pageNumber}/{BrigadeID}/{UnitId}/{CompanyId}/{PlatoonId}")]
	public Task<IActionResult> GetApplicantRoleMappings(int pageNumber, int BrigadeID, int UnitID, int CompanyID, int PlatoonID) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.ApplicantRoleMapping_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _ApplicantRoleMappingRepository.GetApplicantRoleMappings(pageNumber, BrigadeID,  UnitID, CompanyID, PlatoonID);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantRoleMapping_NotFoundList);

		return Ok(result);
	});



	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantRoleMappingById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.ApplicantRoleMapping_InvalidId, id));
		#endregion

		var result = await _ApplicantRoleMappingRepository.GetApplicantRoleMappingById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRoleMapping_NotFoundId, id));

		return Ok(result);
	});



	//[HttpGet("Export"), AllowAnonymous]
	[HttpGet("Export/{BrigadeID}/{UnitId}/{CompanyId}/{PlatoonId}")]
	public Task<IActionResult> Export(int BrigadeID, int UnitId, int CompanyId, int PlatoonId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantRoleMappingRepository.Export(BrigadeID, UnitId, CompanyId, PlatoonId);
		if (result == null)
			return NotFound(ValidationMessages.ApplicantRoleMapping_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicantRoleMapping([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantRoleMappingModel ApplicantRoleMapping = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantRoleMappingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), ApplicantRoleMapping.BrigadeID.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (ApplicantRoleMapping == null) return BadRequest(ValidationMessages.ApplicantRoleMapping_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		//var existingApplicantRoleMapping = await _ApplicantRoleMappingRepository.GetApplicantRoleMappingByName(ApplicantRoleMapping.ApplicantRoleMappingName);
		//if (existingApplicantRoleMapping != null)
		//	return BadRequest(String.Format(ValidationMessages.ApplicantRoleMapping_Duplicate, ApplicantRoleMapping.ApplicantRoleMappingName));
		#endregion

		int insertedApplicantRoleMappingId = await _ApplicantRoleMappingRepository.InsertApplicantRoleMapping(ApplicantRoleMapping, logModel);
		return Created(nameof(GetApplicantRoleMappingById), new { id = insertedApplicantRoleMappingId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicantRoleMapping(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantRoleMappingModel ApplicantRoleMapping = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantRoleMappingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantRoleMapping_InvalidId, id));
		if (ApplicantRoleMapping == null) return BadRequest(ValidationMessages.ApplicantRoleMapping_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != ApplicantRoleMapping.ApplicantRoleMappingId) return BadRequest(ValidationMessages.ApplicantRoleMapping_Mismatch);

		var ApplicantRoleMappingToUpdate = await _ApplicantRoleMappingRepository.GetApplicantRoleMappingById(id);
		if (ApplicantRoleMappingToUpdate == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRoleMapping_NotFoundId, id));
		#endregion

		await _ApplicantRoleMappingRepository.UpdateApplicantRoleMapping(ApplicantRoleMapping, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicantRoleMapping(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.ApplicantRoleMapping_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var ApplicantRoleMappingToDelete = await _ApplicantRoleMappingRepository.GetApplicantRoleMappingById(id);
		if (ApplicantRoleMappingToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicantRoleMapping_NotFoundId, id));
		#endregion

		await _ApplicantRoleMappingRepository.DeleteApplicantRoleMapping(id, logModel);
		return NoContent(); // success
	});
}