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
public partial class CandidateController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CandidateController> _logger;
	private readonly IConfiguration _config;
	private readonly ICandidateRepository _CandidateRepository;
	private readonly ICsvExporter _csvExporter;

	public CandidateController(ISecurityHelper securityHelper, ILogger<CandidateController> logger, IConfiguration config, ICandidateRepository CandidateRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CandidateRepository = CandidateRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet("GetCandidates/{pageNumber}/{UnitId}/{CompanyId}/{PlatoonId}/{TradeId}/{RankId}/{ArmyNo}/{Name}")]
	public Task<IActionResult> GetCandidates(int pageNumber,  int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId,string ArmyNo, string Name ) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Candidate_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CandidateRepository.GetCandidates(pageNumber, UnitId, CompanyId, PlatoonId, TradeId, RankId, ArmyNo, Name);
		if (result == null)
			return NotFound(ValidationMessages.Candidate_NotFoundList);

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

		var result = await _CandidateRepository.GetGroupReport( UnitId, CompanyId, PlatoonId, TradeId, RankId);
		if (result == null)
			return NotFound(ValidationMessages.Candidate_NotFoundList);

		return Ok(result);
	});


	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCandidateById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Candidate_InvalidId, id));
		#endregion

		var result = await _CandidateRepository.GetCandidateById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Candidate_NotFoundId, id));

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

		var result = await _CandidateRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Candidate_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCandidate([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateModel Candidate = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Candidate.CandidateName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Candidate == null) return BadRequest(ValidationMessages.Candidate_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCandidate = await _CandidateRepository.GetCandidateByName(Candidate.CandidateName);
		if (existingCandidate != null)
			return BadRequest(String.Format(ValidationMessages.Candidate_Duplicate, Candidate.CandidateName));
		#endregion

		int insertedCandidateId = await _CandidateRepository.InsertCandidate(Candidate, logModel);
		return Created(nameof(GetCandidateById), new { id = insertedCandidateId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCandidate(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CandidateModel Candidate = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CandidateModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Candidate_InvalidId, id));
		if (Candidate == null) return BadRequest(ValidationMessages.Candidate_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Candidate.CandidateId) return BadRequest(ValidationMessages.Candidate_Mismatch);

		var CandidateToUpdate = await _CandidateRepository.GetCandidateById(id);
		if (CandidateToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Candidate_NotFoundId, id));
		#endregion

		await _CandidateRepository.UpdateCandidate(Candidate, logModel);
		return NoContent(); // success
	});


}