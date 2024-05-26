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
public partial class ApplicantController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicantController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicantRepository _ApplicantRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicantController(ISecurityHelper securityHelper, ILogger<ApplicantController> logger, IConfiguration config, IApplicantRepository ApplicantRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._ApplicantRepository = ApplicantRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet("GetApplicants/{pageNumber}/{BrigadeID}/{UnitId}/{CompanyId}/{PlatoonId}/{TradeId}/{RankId}/{SoldierUserId}/{ArmyNo}/{Name}")]
	public Task<IActionResult> GetApplicants(int pageNumber,int BrigadeID,  int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string SoldierUserId, string ArmyNo, string Name ) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Applicant_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _ApplicantRepository.GetApplicants(pageNumber, BrigadeID, UnitId, CompanyId, PlatoonId, TradeId, RankId, SoldierUserId, ArmyNo, Name);
		if (result == null)
			return NotFound(ValidationMessages.Applicant_NotFoundList);

		return Ok(result);
	});



	[HttpGet("GetGroupReport/{UnitId}/{CompanyId}/{PlatoonId}/{TradeId}/{RankId}")]
	public Task<IActionResult> GetGroupReport( int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), UnitId + CompanyId + PlatoonId + TradeId + RankId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

	
		#endregion

		var result = await _ApplicantRepository.GetGroupReport( UnitId, CompanyId, PlatoonId, TradeId, RankId);
		if (result == null)
			return NotFound(ValidationMessages.Applicant_NotFoundList);

		return Ok(result);
	});


	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetApplicantById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Applicant_InvalidId, id));
		#endregion

		var result = await _ApplicantRepository.GetApplicantById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Applicant_NotFoundId, id));

		return Ok(result);
	});

	//[HttpGet("{id:string}"), AllowAnonymous]
	[HttpGet("GetApplicantByUserId/{id}")]
	public Task<IActionResult> GetApplicantByUserId(string id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id == "" || id == null)
			return BadRequest(String.Format(ValidationMessages.Applicant_InvalidId, id));
		#endregion

		var result = await _ApplicantRepository.GetApplicantByUserId(id);
		if (result == null)
			return Ok(new ApplicantModel());
			//return NotFound(String.Format(ValidationMessages.Applicant_NotFoundId, id));

		return Ok(result);
	});



	//[HttpGet("Export"), AllowAnonymous]
	[HttpGet("Export/{BrigadeID}/{UnitId}/{CompanyId}/{PlatoonId}/{TradeId}/{RankId}/{SoldierUserId}/{ArmyNo}/{Name}")]
	public Task<IActionResult> Export(int BrigadeID, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string SoldierUserId, string ArmyNo, string Name) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _ApplicantRepository.Export(BrigadeID, UnitId, CompanyId, PlatoonId, TradeId, RankId, SoldierUserId, ArmyNo, Name);
		if (result == null)
			return NotFound(ValidationMessages.Applicant_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertApplicant([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantModel Applicant = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Applicant.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Applicant == null) return BadRequest(ValidationMessages.Applicant_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingApplicant = await _ApplicantRepository.GetApplicantByName(Applicant.Name);
		if (existingApplicant != null)
			return BadRequest(String.Format(ValidationMessages.Applicant_Duplicate, Applicant.Name));
		#endregion

		int insertedApplicantId = await _ApplicantRepository.InsertApplicant(Applicant, logModel);
		return Created(nameof(GetApplicantById), new { id = insertedApplicantId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateApplicant(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		ApplicantModel Applicant = PostData["Data"] == null ? null : JsonSerializer.Deserialize<ApplicantModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Applicant_InvalidId, id));
		if (Applicant == null) return BadRequest(ValidationMessages.Applicant_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Applicant.ApplicantId) return BadRequest(ValidationMessages.Applicant_Mismatch);

		var ApplicantToUpdate = await _ApplicantRepository.GetApplicantById(id);
		if (ApplicantToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Applicant_NotFoundId, id));
		#endregion

		await _ApplicantRepository.UpdateApplicant(Applicant, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteApplicant(int id, [FromBody] LogModel logModel) =>
		TryCatch(async () =>
		{
			#region Validation
			if (Convert.ToBoolean(_config["Hash:HashChecking"]))
			{
				if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
					return Unauthorized(ValidationMessages.InvalidHash);
			}

			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (id <= 0) return BadRequest(String.Format(ValidationMessages.Applicant_InvalidId, id));
			if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

			var ApplicantToDelete = await _ApplicantRepository.GetApplicantById(id);
			if (ApplicantToDelete == null)
				return NotFound(String.Format(ValidationMessages.Applicant_NotFoundId, id));

			#endregion

			await _ApplicantRepository.DeleteApplicant(id, logModel);
			return NoContent(); // success
		});
}