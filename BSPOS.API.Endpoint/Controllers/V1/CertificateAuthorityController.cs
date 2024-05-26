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
public partial class CertificateAuthorityController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CertificateAuthorityController> _logger;
	private readonly IConfiguration _config;
	private readonly ICertificateAuthorityRepository _CertificateAuthorityRepository;
	private readonly ICsvExporter _csvExporter;

	public CertificateAuthorityController(ISecurityHelper securityHelper, ILogger<CertificateAuthorityController> logger, IConfiguration config, ICertificateAuthorityRepository CertificateAuthorityRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CertificateAuthorityRepository = CertificateAuthorityRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCertificateAuthoritys(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.CertificateAuthority_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CertificateAuthorityRepository.GetCertificateAuthoritys(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.CertificateAuthority_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCertificateAuthoritys"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCertificateAuthoritys() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CertificateAuthorityRepository.GetDistinctCertificateAuthoritys();
		if (result == null)
			return NotFound(ValidationMessages.CertificateAuthority_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCertificateAuthorityById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.CertificateAuthority_InvalidId, id));
		#endregion

		var result = await _CertificateAuthorityRepository.GetCertificateAuthorityById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.CertificateAuthority_NotFoundId, id));

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

		var result = await _CertificateAuthorityRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.CertificateAuthority_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertCertificateAuthority([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CertificateAuthorityModel CertificateAuthority = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CertificateAuthorityModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), CertificateAuthority.CertificateAuthorityName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (CertificateAuthority == null) return BadRequest(ValidationMessages.CertificateAuthority_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCertificateAuthority = await _CertificateAuthorityRepository.GetCertificateAuthorityByName(CertificateAuthority.CertificateAuthorityName);
		if (existingCertificateAuthority != null)
			return BadRequest(String.Format(ValidationMessages.CertificateAuthority_Duplicate, CertificateAuthority.CertificateAuthorityName));
		#endregion

		int insertedCertificateAuthorityId = await _CertificateAuthorityRepository.InsertCertificateAuthority(CertificateAuthority, logModel);
		return Created(nameof(GetCertificateAuthorityById), new { id = insertedCertificateAuthorityId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateCertificateAuthority(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CertificateAuthorityModel CertificateAuthority = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CertificateAuthorityModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CertificateAuthority_InvalidId, id));
		if (CertificateAuthority == null) return BadRequest(ValidationMessages.CertificateAuthority_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != CertificateAuthority.CertificateAuthorityId) return BadRequest(ValidationMessages.CertificateAuthority_Mismatch);

		var CertificateAuthorityToUpdate = await _CertificateAuthorityRepository.GetCertificateAuthorityById(id);
		if (CertificateAuthorityToUpdate == null)
			return NotFound(String.Format(ValidationMessages.CertificateAuthority_NotFoundId, id));
		#endregion

		await _CertificateAuthorityRepository.UpdateCertificateAuthority(CertificateAuthority, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteCertificateAuthority(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.CertificateAuthority_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CertificateAuthorityToDelete = await _CertificateAuthorityRepository.GetCertificateAuthorityById(id);
		if (CertificateAuthorityToDelete == null)
			return NotFound(String.Format(ValidationMessages.CertificateAuthority_NotFoundId, id));
		#endregion

		await _CertificateAuthorityRepository.DeleteCertificateAuthority(id, logModel);
		return NoContent(); // success
	});
}