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
public partial class BAASectionController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<BAASectionController> _logger;
	private readonly IConfiguration _config;
	private readonly IBAASectionRepository _BAASectionRepository;
	private readonly ICsvExporter _csvExporter;

	public BAASectionController(ISecurityHelper securityHelper, ILogger<BAASectionController> logger, IConfiguration config, IBAASectionRepository BAASectionRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._BAASectionRepository = BAASectionRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetBAASections(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.BAASection_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _BAASectionRepository.GetBAASections(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.BAASection_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctBAASections"), AllowAnonymous]
	public Task<IActionResult> GetDistinctBAASections() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _BAASectionRepository.GetDistinctBAASections();
		if (result == null)
			return NotFound(ValidationMessages.BAASection_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetBAASectionById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.BAASection_InvalidId, id));
		#endregion

		var result = await _BAASectionRepository.GetBAASectionById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.BAASection_NotFoundId, id));

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

		var result = await _BAASectionRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.BAASection_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertBAASection([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BAASectionModel BAASection = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BAASectionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), BAASection.BAASectionName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (BAASection == null) return BadRequest(ValidationMessages.BAASection_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingBAASection = await _BAASectionRepository.GetBAASectionByName(BAASection.BAASectionName);
		if (existingBAASection != null)
			return BadRequest(String.Format(ValidationMessages.BAASection_Duplicate, BAASection.BAASectionName));
		#endregion

		int insertedBAASectionId = await _BAASectionRepository.InsertBAASection(BAASection, logModel);
		return Created(nameof(GetBAASectionById), new { id = insertedBAASectionId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateBAASection(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		BAASectionModel BAASection = PostData["Data"] == null ? null : JsonSerializer.Deserialize<BAASectionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.BAASection_InvalidId, id));
		if (BAASection == null) return BadRequest(ValidationMessages.BAASection_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != BAASection.BAASectionId) return BadRequest(ValidationMessages.BAASection_Mismatch);

		var BAASectionToUpdate = await _BAASectionRepository.GetBAASectionById(id);
		if (BAASectionToUpdate == null)
			return NotFound(String.Format(ValidationMessages.BAASection_NotFoundId, id));
		#endregion

		await _BAASectionRepository.UpdateBAASection(BAASection, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteBAASection(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.BAASection_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var BAASectionToDelete = await _BAASectionRepository.GetBAASectionById(id);
		if (BAASectionToDelete == null)
			return NotFound(String.Format(ValidationMessages.BAASection_NotFoundId, id));
		#endregion

		await _BAASectionRepository.DeleteBAASection(id, logModel);
		return NoContent(); // success
	});
}