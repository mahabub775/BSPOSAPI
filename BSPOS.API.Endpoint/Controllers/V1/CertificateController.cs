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
public partial class CertificateController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CertificateController> _logger;
	private readonly IConfiguration _config;
	private readonly ICertificateRepository _CertificateRepository;
	private readonly ICsvExporter _csvExporter;

	public CertificateController(ISecurityHelper securityHelper, ILogger<CertificateController> logger, IConfiguration config, ICertificateRepository CertificateRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CertificateRepository = CertificateRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCertificates(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Certificate_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CertificateRepository.GetCertificates(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Certificate_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCertificates"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCertificates() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CertificateRepository.GetDistinctCertificates();
		if (result == null)
			return NotFound(ValidationMessages.Certificate_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCertificateById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Certificate_InvalidId, id));
		#endregion

		var result = await _CertificateRepository.GetCertificateById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Certificate_NotFoundId, id));

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

		var result = await _CertificateRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Certificate_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertCertificate([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CertificateModel Certificate = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CertificateModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Certificate.CertificateName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Certificate == null) return BadRequest(ValidationMessages.Certificate_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCertificate = await _CertificateRepository.GetCertificateByName(Certificate.CertificateName);
		if (existingCertificate != null)
			return BadRequest(String.Format(ValidationMessages.Certificate_Duplicate, Certificate.CertificateName));
		#endregion

		int insertedCertificateId = await _CertificateRepository.InsertCertificate(Certificate, logModel);
		return Created(nameof(GetCertificateById), new { id = insertedCertificateId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateCertificate(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CertificateModel Certificate = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CertificateModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Certificate_InvalidId, id));
		if (Certificate == null) return BadRequest(ValidationMessages.Certificate_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Certificate.CertificateId) return BadRequest(ValidationMessages.Certificate_Mismatch);

		var CertificateToUpdate = await _CertificateRepository.GetCertificateById(id);
		if (CertificateToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Certificate_NotFoundId, id));
		#endregion

		await _CertificateRepository.UpdateCertificate(Certificate, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteCertificate(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Certificate_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CertificateToDelete = await _CertificateRepository.GetCertificateById(id);
		if (CertificateToDelete == null)
			return NotFound(String.Format(ValidationMessages.Certificate_NotFoundId, id));
		#endregion

		await _CertificateRepository.DeleteCertificate(id, logModel);
		return NoContent(); // success
	});
}