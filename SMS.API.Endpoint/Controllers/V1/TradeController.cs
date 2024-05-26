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
public partial class TradeController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<TradeController> _logger;
	private readonly IConfiguration _config;
	private readonly ITradeRepository _TradeRepository;
	private readonly ICsvExporter _csvExporter;

	public TradeController(ISecurityHelper securityHelper, ILogger<TradeController> logger, IConfiguration config, ITradeRepository TradeRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._TradeRepository = TradeRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public Task<IActionResult> GetTrades(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Trade_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _TradeRepository.GetTrades(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Trade_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctTrades"), AllowAnonymous]
	public Task<IActionResult> GetDistinctTrades() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _TradeRepository.GetDistinctTrades();
		if (result == null)
			return NotFound(ValidationMessages.Trade_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetTradeById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Trade_InvalidId, id));
		#endregion

		var result = await _TradeRepository.GetTradeById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Trade_NotFoundId, id));

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

		var result = await _TradeRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Trade_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertTrade([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		TradeModel Trade = PostData["Data"] == null ? null : JsonSerializer.Deserialize<TradeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Trade.TradeName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (Trade == null) return BadRequest(ValidationMessages.Trade_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var existingTrade = await _TradeRepository.GetTradeByName(Trade.TradeName);
		if (existingTrade != null)
			return BadRequest(String.Format(ValidationMessages.Trade_Duplicate, Trade.TradeName));
		#endregion

		int insertedTradeId = await _TradeRepository.InsertTrade(Trade, logModel);
		return Created(nameof(GetTradeById), new { id = insertedTradeId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateTrade(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		TradeModel Trade = PostData["Data"] == null ? null : JsonSerializer.Deserialize<TradeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Trade_InvalidId, id));
		if (Trade == null) return BadRequest(ValidationMessages.Trade_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != Trade.TradeId) return BadRequest(ValidationMessages.Trade_Mismatch);

		var TradeToUpdate = await _TradeRepository.GetTradeById(id);
		if (TradeToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Trade_NotFoundId, id));
		#endregion

		await _TradeRepository.UpdateTrade(Trade, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteTrade(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Trade_InvalidId, id));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		var TradeToDelete = await _TradeRepository.GetTradeById(id);
		if (TradeToDelete == null)
			return NotFound(String.Format(ValidationMessages.Trade_NotFoundId, id));
		#endregion

		await _TradeRepository.DeleteTrade(id, logModel);
		return NoContent(); // success
	});
}