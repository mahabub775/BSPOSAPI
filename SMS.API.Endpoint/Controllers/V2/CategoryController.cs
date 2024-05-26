using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

namespace SMS.API.Endpoint.Controllers.V2;

[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class CategoryController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<CategoryController> _logger;
	private readonly IConfiguration _config;
	private readonly ICategoryRepository _categoryRepository;
	private readonly ICsvExporter _csvExporter;

	public CategoryController(ISecurityHelper securityHelper, ILogger<CategoryController> logger, IConfiguration config, ICategoryRepository categoryRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._categoryRepository = categoryRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	public Task<IActionResult> GetCategories(int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Category_InvalidPageNumber, pageNumber));

		var result = await _categoryRepository.GetCategories(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(result);
	});

	[HttpGet("GetDistinctCategories"), AllowAnonymous]
	public Task<IActionResult> GetDistinctCategories() =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _categoryRepository.GetDistinctCategories();

		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetCategoryById(int id) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Category_InvalidId, id));

		var result = await _categoryRepository.GetCategoryById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Category_NotFoundId, id));

		return Ok(result);
	});

	[HttpGet("GetCategoriesWithPies"), AllowAnonymous]
	public Task<IActionResult> GetCategoriesWithPies() =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _categoryRepository.GetCategoriesWithPies();
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundGetCategoriesWithPies);

		return Ok(result);
	});

	[HttpGet("Export"), AllowAnonymous]
	public Task<IActionResult> Export() =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _categoryRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertCategory([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		CategoryModel category = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CategoryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), category.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (category == null) return BadRequest(ValidationMessages.Category_Null);

		var existingCategory = await _categoryRepository.GetCategoryByName(category.Name);
		if (existingCategory != null)
		{
			ModelState.AddModelError("Duplicate Category", String.Format(ValidationMessages.Category_Duplicate, category.Name));
			return BadRequest(String.Format(ValidationMessages.Category_Duplicate, category.Name));
		}
		#endregion

		int insertedCategoryId = await _categoryRepository.InsertCategory(category, logModel);
		return Created(nameof(GetCategoryById), new { id = insertedCategoryId });
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateCategory(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Category_InvalidId, id));

		CategoryModel category = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CategoryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (category == null) return BadRequest(ValidationMessages.Category_Null);

		if (id != category.Id) return BadRequest(ValidationMessages.Category_Mismatch);

		var categoryToUpdate = await _categoryRepository.GetCategoryById(id);
		if (categoryToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Category_NotFoundId, id));
		#endregion

		await _categoryRepository.UpdateCategory(category, logModel);
		return NoContent(); // success
	});

	[HttpPut("Delete/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteCategory(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Category_InvalidId, id));

		var categoryToDelete = await _categoryRepository.GetCategoryById(id);
		if (categoryToDelete == null)
			return NotFound(String.Format(ValidationMessages.Category_NotFoundId, id));
		#endregion

		await _categoryRepository.DeleteCategory(id, logModel);
		return NoContent(); // success
	});
}