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
public partial class UnitController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<UnitController> _logger;
	private readonly IConfiguration _config;
	private readonly IUnitRepository _UnitRepository;
	private readonly ICsvExporter _csvExporter;

	public UnitController(ISecurityHelper securityHelper, ILogger<UnitController> logger, IConfiguration config, IUnitRepository UnitRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._UnitRepository = UnitRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetUnits(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Unit_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _UnitRepository.GetUnits(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Unit_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctUnits"), AllowAnonymous]
	public Task<IActionResult> GetDistinctUnits() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _UnitRepository.GetDistinctUnits();
		if (result == null)
			return NotFound(ValidationMessages.Unit_NotFoundList);

		return Ok(result);
	});



	[HttpGet("GetUnitsByBrigade/{BrigadeId:int}"), AllowAnonymous]
	public Task<IActionResult> GetUnitsByBrigade(int BrigadeId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _UnitRepository.GetUnitsByBrigade(BrigadeId);
		if (result == null)
			return NotFound(ValidationMessages.Unit_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetUnitById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Unit_InvalidId, id));
		#endregion

		var result = await _UnitRepository.GetUnitById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Unit_NotFoundId, id));

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

		var result = await _UnitRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Unit_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost]
	public Task<IActionResult> InsertUnit([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		UnitModel Unit = PostData["Data"] == null ? null : JsonSerializer.Deserialize<UnitModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Unit.UnitName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Unit == null) return BadRequest(ValidationMessages.Unit_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingUnit = await _UnitRepository.GetUnitByName(Unit.UnitName);
		if (existingUnit != null)
			return BadRequest(String.Format(ValidationMessages.Unit_Duplicate, Unit.UnitName));
		#endregion

		int insertedUnitId = await _UnitRepository.InsertUnit(Unit, logModel);
		return Created(nameof(GetUnitById), new { id = insertedUnitId });
	});

	[HttpPut("Update/{id:int}")]
	public Task<IActionResult> UpdateUnit(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		UnitModel Unit = PostData["Data"] == null ? null : JsonSerializer.Deserialize<UnitModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Unit_InvalidId, id));
		if (Unit == null) return BadRequest(ValidationMessages.Unit_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Unit.UnitId) return BadRequest(ValidationMessages.Unit_Mismatch);

		var UnitToUpdate = await _UnitRepository.GetUnitById(id);
		if (UnitToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Unit_NotFoundId, id));
		#endregion

		await _UnitRepository.UpdateUnit(Unit, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}")]
	public Task<IActionResult> DeleteUnit(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Unit_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var UnitToDelete = await _UnitRepository.GetUnitById(id);
		if (UnitToDelete == null)
			return NotFound(String.Format(ValidationMessages.Unit_NotFoundId, id));
		#endregion

		await _UnitRepository.DeleteUnit(id, logModel);
		return NoContent(); // success
	});
}