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
public partial class CompanyController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CompanyController> _logger;
	private readonly IConfiguration _config;
	private readonly ICompanyRepository _CompanyRepository;
	private readonly ICsvExporter _csvExporter;

	public CompanyController(ISecurityHelper securityHelper, ILogger<CompanyController> logger, IConfiguration config, ICompanyRepository CompanyRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CompanyRepository = CompanyRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCompanys(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Company_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CompanyRepository.GetCompanys(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Company_NotFoundList);

		return Ok(result);
	});

	
	[HttpGet("GetCompanysByUnit/{UnitId:int}"), AllowAnonymous]
	public Task<IActionResult> GetCompanysByUnit(int UnitId) =>
TryCatch(async () =>
{
	#region Validation
	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	{
		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
			return Unauthorized(ValidationMessages.InvalidHash);
	}

	#endregion

	var result = await _CompanyRepository.GetCompanysByUnit(UnitId);
	if (result == null)
		return NotFound(ValidationMessages.Unit_NotFoundList);

	return Ok(result);
});


	[HttpGet("GetDistinctCompanys"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCompanys() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CompanyRepository.GetDistinctCompanys();
		if (result == null)
			return NotFound(ValidationMessages.Company_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCompanyById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Company_InvalidId, id));
		#endregion

		var result = await _CompanyRepository.GetCompanyById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Company_NotFoundId, id));

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

		var result = await _CompanyRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Company_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertCompany([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CompanyModel Company = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CompanyModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Company.CompanyName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Company == null) return BadRequest(ValidationMessages.Company_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCompany = await _CompanyRepository.GetCompanyByName(Company.CompanyName);
		if (existingCompany != null)
			return BadRequest(String.Format(ValidationMessages.Company_Duplicate, Company.CompanyName));
		#endregion

		int insertedCompanyId = await _CompanyRepository.InsertCompany(Company, logModel);
		return Created(nameof(GetCompanyById), new { id = insertedCompanyId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateCompany(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CompanyModel Company = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CompanyModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Company_InvalidId, id));
		if (Company == null) return BadRequest(ValidationMessages.Company_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Company.CompanyId) return BadRequest(ValidationMessages.Company_Mismatch);

		var CompanyToUpdate = await _CompanyRepository.GetCompanyById(id);
		if (CompanyToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Company_NotFoundId, id));
		#endregion

		await _CompanyRepository.UpdateCompany(Company, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteCompany(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Company_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CompanyToDelete = await _CompanyRepository.GetCompanyById(id);
		if (CompanyToDelete == null)
			return NotFound(String.Format(ValidationMessages.Company_NotFoundId, id));
		#endregion

		await _CompanyRepository.DeleteCompany(id, logModel);
		return NoContent(); // success
	});
}