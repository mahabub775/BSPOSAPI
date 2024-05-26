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
public partial class DeshboardController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<DeshboardController> _logger;
	private readonly IConfiguration _config;
	private readonly IDeshboardRepository _DeshboardRepository;
	private readonly ICsvExporter _csvExporter;

	public DeshboardController(ISecurityHelper securityHelper, ILogger<DeshboardController> logger, IConfiguration config, IDeshboardRepository DeshboardRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._DeshboardRepository = DeshboardRepository;
		this._csvExporter = csvExporter;
	}



	[HttpGet, AllowAnonymous]
	public Task<IActionResult> GetDeshboardData() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), 1.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		
		#endregion

		var result = await _DeshboardRepository.GetDeshboardData();
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Deshboard_DataNotFound));

		return Ok(result);
	});


}