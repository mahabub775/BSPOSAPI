using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BSPOS.API.Endpoint.Resources;
using BSPOS.Core.Constant;
using BSPOS.Core.Contract.Infrastructure;
using BSPOS.Core.Contract.Persistence;
using BSPOS.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BSPOS.API.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace BSPOS.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class UserController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<UserController> _logger;
	private readonly IConfiguration _config;
	private readonly RoleManager<IdentityRole> _RoleManager;
	private readonly SignInManager<ApplicationUser> _SignInManager;


	public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ISecurityHelper securityHelper, ILogger<UserController> logger, IConfiguration config,  ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		_userManager = userManager;
		_SignInManager = signInManager;
		_RoleManager = roleManager;

	}


	#region User

	[HttpGet("GetUsers"), AllowAnonymous]
	public Task<IActionResult> GetUsers() =>
	TryCatch(async () =>
	{
		var tempUsers = await _userManager.Users.ToListAsync();

		List<ApplicationUser> Users = new List<ApplicationUser>();
		foreach (ApplicationUser oItem in tempUsers)
		{
			var CurrentUserroles = await _userManager.GetRolesAsync(oItem);
			var user = new ApplicationUser
			{
				Id = oItem.Id,
				UserName = oItem.UserName,
				FullName = oItem.FullName,
				Email = oItem.Email,
				PhoneNumber = oItem.PhoneNumber,
				Address = oItem.Address,
				RoleName = CurrentUserroles != null && CurrentUserroles.Count() > 0 ? CurrentUserroles[0] : ""
			};

			Users.Add(user);
		}


		return Ok(Users);
	});

	[HttpGet("GetUserById"), AllowAnonymous]
	[EnableRateLimiting("LimiterPolicy")]
	public  Task<IActionResult> GetUserById(string Userid) =>
	TryCatch(async () =>
		{

			var result = await _userManager.FindByIdAsync(Userid);
			return Ok(result);
		});
	[HttpDelete("DeleteUser")]
	public  Task<IActionResult> DeleteUser(string Userid)=>
	TryCatch(async () =>
	{ 
		var oUser = await _userManager.FindByIdAsync(Userid);
		var result = await _userManager.DeleteAsync(oUser);

		return Ok(result);
	});

	[HttpPost("Registration")]
	public  Task<IActionResult> Registration(UserInfoModel oModel) =>
	TryCatch(async () =>
	{

		var oUser = new ApplicationUser { UserName = oModel.UserName, Email = oModel.Email, FullName = oModel.Name, PhoneNumber = oModel.PhoneNumber, Address = oModel.Address };
		var result = await _userManager.CreateAsync(oUser, oModel.Password);
		if (result.Succeeded)
		{
			return Ok(new { message = "1" });
		}
		else
		{
			string sErrorMessage = string.Empty;
			foreach (var error in result.Errors)
			{
				sErrorMessage += error.Description;
			}
			return Ok(new { message = sErrorMessage });
		}

	});


	[HttpPut("UpdateUser"), AllowAnonymous]
	public  Task<IActionResult> Updateuser(ApplicationUser oUser) =>
		TryCatch(async () =>
		{
			ApplicationUser oExistUser = (ApplicationUser)await _userManager.FindByIdAsync(oUser.Id);
			if (oExistUser == null)
			{
				return NotFound();
			}
			oExistUser.UserName = oUser.UserName;
			oExistUser.Email = oUser.Email;
			oExistUser.FullName = oUser.FullName;
			oExistUser.PhoneNumber = oUser.PhoneNumber;
			oExistUser.Address = oUser.Address;

			var result = await _userManager.UpdateAsync(oExistUser);
			if (result.Succeeded)
			{
				return Ok(new { message = "2" });
			}
			else
			{
				string sErrorMessage = string.Empty;
				foreach (var error in result.Errors)
				{
					sErrorMessage += error.Description;
				}
				return Ok(new { message = sErrorMessage });
			}

		});


	[HttpPut("ChangePassword"), AllowAnonymous]
	public  Task<IActionResult> ChangePassword(ChangePasswordModel oModel) =>
			TryCatch(async () =>
			{
				var user = await _userManager.FindByIdAsync(oModel.UserId);

				if (user == null)
				{
					return NotFound($"User with ID {oModel.UserId} not found.");
				}

				var result = await _userManager.ChangePasswordAsync(user, oModel.CurrentPassword, oModel.NewPassword);

				if (result.Succeeded)
				{
					return Ok();
				}
				else
				{
					return BadRequest(result.Errors);
				}
			});

	#endregion


	//[HttpGet, AllowAnonymous]
	//[EnableRateLimiting("LimiterPolicy")]
	//public Task<IActionResult> GetUsers(int pageNumber) =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}

	//	if (pageNumber < 0)
	//		return BadRequest(String.Format(ValidationMessages.User_InvalidPageNumber, pageNumber));
	//	#endregion

	//	var result = await _UserRepository.GetUsers(pageNumber);
	//	if (result == null)
	//		return NotFound(ValidationMessages.User_NotFoundList);

	//	return Ok(result);
	//});

	//[HttpGet("GetDistinctUsers"), AllowAnonymous]
	//public Task<IActionResult> GetDistinctUsers() =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}
	//	#endregion

	//	var result = await _UserRepository.GetDistinctUsers();
	//	if (result == null)
	//		return NotFound(ValidationMessages.User_NotFoundList);

	//	return Ok(result);
	//});

	//[HttpGet("{id:int}"), AllowAnonymous]
	//public Task<IActionResult> GetUserById(int id) =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}

	//	if (id < 1)
	//		return BadRequest(String.Format(ValidationMessages.User_InvalidId, id));
	//	#endregion

	//	var result = await _UserRepository.GetUserById(id);
	//	if (result == null)
	//		return NotFound(String.Format(ValidationMessages.User_NotFoundId, id));

	//	return Ok(result);
	//});

	//[HttpGet("GetUsersWithPies"), AllowAnonymous]
	//public Task<IActionResult> GetUsersWithPies() =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}
	//	#endregion

	//	var result = await _UserRepository.GetUsersWithPies();
	//	if (result == null)
	//		return NotFound(ValidationMessages.User_NotFoundGetUsersWithPies);

	//	return Ok(result);
	//});


	//[HttpPost]
	//public Task<IActionResult> InsertUser([FromBody] Dictionary<string, object> PostData) =>
	//TryCatch(async () =>
	//{
	//	UserModel User = PostData["Data"] == null ? null : JsonSerializer.Deserialize<UserModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
	//	LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), User.Name))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}

	//	if (User == null) return BadRequest(ValidationMessages.User_Null);
	//	if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

	//	var existingUser = await _UserRepository.GetUserByName(User.Name);
	//	if (existingUser != null)
	//		return BadRequest(String.Format(ValidationMessages.User_Duplicate, User.Name));
	//	#endregion

	//	int insertedUserId = await _UserRepository.InsertUser(User, logModel);
	//	return Created(nameof(GetUserById), new { id = insertedUserId });
	//});

	//[HttpPut("Update/{id:int}")]
	//public Task<IActionResult> UpdateUser(int id, [FromBody] Dictionary<string, object> PostData) =>
	//TryCatch(async () =>
	//{
	//	UserModel User = PostData["Data"] == null ? null : JsonSerializer.Deserialize<UserModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
	//	LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}

	//	if (id <= 0) return BadRequest(String.Format(ValidationMessages.User_InvalidId, id));
	//	if (User == null) return BadRequest(ValidationMessages.User_Null);
	//	if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
	//	if (id != User.Id) return BadRequest(ValidationMessages.User_Mismatch);

	//	var UserToUpdate = await _UserRepository.GetUserById(id);
	//	if (UserToUpdate == null)
	//		return NotFound(String.Format(ValidationMessages.User_NotFoundId, id));
	//	#endregion

	//	await _UserRepository.UpdateUser(User, logModel);
	//	return NoContent(); // success
	//});

	//[HttpPut("Delete/{id:int}")]
	//public Task<IActionResult> DeleteUser(int id, [FromBody] LogModel logModel) =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}

	//	if (!ModelState.IsValid) return BadRequest(ModelState);

	//	if (id <= 0) return BadRequest(String.Format(ValidationMessages.User_InvalidId, id));
	//	if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

	//	var UserToDelete = await _UserRepository.GetUserById(id);
	//	if (UserToDelete == null)
	//		return NotFound(String.Format(ValidationMessages.User_NotFoundId, id));
	//	#endregion

	//	await _UserRepository.DeleteUser(id, logModel);
	//	return NoContent(); // success
	//});
}