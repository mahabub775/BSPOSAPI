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
public partial class CountryController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CountryController> _logger;
	private readonly IConfiguration _config;
	private readonly ICountryRepository _CountryRepository;
	private readonly ICsvExporter _csvExporter;

	public CountryController(ISecurityHelper securityHelper, ILogger<CountryController> logger, IConfiguration config, ICountryRepository CountryRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._CountryRepository = CountryRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetCountrys(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Country_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _CountryRepository.GetCountrys(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Country_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCountrys"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCountrys() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _CountryRepository.GetDistinctCountrys();
		if (result == null)
			return NotFound(ValidationMessages.Country_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCountryById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Country_InvalidId, id));
		#endregion

		var result = await _CountryRepository.GetCountryById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Country_NotFoundId, id));

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

		var result = await _CountryRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Country_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCountry([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CountryModel Country = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CountryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Country.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Country == null) return BadRequest(ValidationMessages.Country_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingCountry = await _CountryRepository.GetCountryByName(Country.Name);
		if (existingCountry != null)
			return BadRequest(String.Format(ValidationMessages.Country_Duplicate, Country.Name));
		#endregion

		int insertedCountryId = await _CountryRepository.InsertCountry(Country, logModel);
		return Created(nameof(GetCountryById), new { id = insertedCountryId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCountry(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CountryModel Country = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CountryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Country_InvalidId, id));
		if (Country == null) return BadRequest(ValidationMessages.Country_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Country.CountryId) return BadRequest(ValidationMessages.Country_Mismatch);

		var CountryToUpdate = await _CountryRepository.GetCountryById(id);
		if (CountryToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Country_NotFoundId, id));
		#endregion

		await _CountryRepository.UpdateCountry(Country, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCountry(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Country_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var CountryToDelete = await _CountryRepository.GetCountryById(id);
		if (CountryToDelete == null)
			return NotFound(String.Format(ValidationMessages.Country_NotFoundId, id));
		#endregion

		await _CountryRepository.DeleteCountry(id, logModel);
		return NoContent(); // success
	});
}