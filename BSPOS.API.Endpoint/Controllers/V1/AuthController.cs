using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BSPOS.API.Endpoint.Resources;
using BSPOS.Core.Contract.Infrastructure;
using BSPOS.Core.Contract.Persistence;
using BSPOS.Core.Model;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BSPOS.API.Persistence.Identity;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;

namespace BSPOS.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> _logger;
	private readonly IConfiguration _config;
	private readonly ISecurityHelper _securityHelper;
	private readonly IAuthRepository _authRepository;

	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _RoleManager;
	private readonly SignInManager<ApplicationUser> _SignInManager;

	public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ILogger<AuthController> logger, IConfiguration config, ISecurityHelper securityHelper, IAuthRepository authRepository)
	{
		this._logger = logger;
		this._config = config;
		this._securityHelper = securityHelper;
		this._authRepository = authRepository;
		_userManager = userManager;
		_SignInManager = signInManager;
		_RoleManager = roleManager;
	}

	[HttpPost("Login"), AllowAnonymous]
	public Task<IActionResult> Login(UserLoginModel oModel) =>
	TryCatch(async () =>
	{
		#region Validation
		//if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		//{
		//	if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), oModel.Id))
		//		return Unauthorized(ValidationMessages.InvalidHash);
		//}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (oModel == null)
			return BadRequest(ValidationMessages.Auth_UserInfoNull);
		#endregion

		var user = await _userManager.FindByNameAsync(oModel.UserName);
		if (user == null)
		{
			return NotFound();
		}
		if (user != null && await _userManager.CheckPasswordAsync(user, oModel.Password))
		{
			var userRoles = await _userManager.GetRolesAsync(user);


			var oUserInfoModel = new UserInfoModel();
			oUserInfoModel.Id = user.Id;
			oUserInfoModel.UserName = user.UserName;
			oUserInfoModel.Name = user.FullName;
			oUserInfoModel.Email = user.Email;
			oUserInfoModel.PhoneNumber = user.PhoneNumber;
			oUserInfoModel.Role = userRoles != null && userRoles.Count() > 0 ? userRoles[0] : "";
			user.RoleName = userRoles != null && userRoles.Count() > 0 ? userRoles[0] : "";
			return Ok(new
			{
				user = user,
				userRoles = userRoles,
				token = GetToken(oUserInfoModel)

				//expiration = token.ValidTo
			});
		}

		return Unauthorized();
	});

	private async Task<TokenModel> GetToken(UserInfoModel userInfo)
	{

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _authRepository.UpdateRefreshToken(userInfo.Id, token);
		return token;

	}

	[HttpGet("LogOut"), AllowAnonymous]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
		return Ok(new { message = "LogOut" });
	}




	[HttpGet("GetCurrentUser")]
	public IActionResult GetCurrentUser() =>
	TryCatch(() =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var identity = HttpContext.User.Identity as ClaimsIdentity;

		if (identity != null)
		{
			var claims = identity.Claims;
			var userModel = new UserInfoModel
			{
				Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
				UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
				Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
				Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
			};

			return Ok(userModel);
		}

		return null;
	});


	[HttpPost("RefreshToken"), AllowAnonymous]
	public Task<IActionResult> RefreshToken([FromBody] string userId) =>
	TryCatch(async () =>
	{
		#region Validatation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		UserInfoModel userInfo = await _authRepository.GetCurrentUser(userId);
		if (userInfo == null) return Unauthorized();

		TokenModel refreshToken = await _authRepository.GetRefreshToken(userId);

		if (Request.Cookies["X-RefreshToken"] != null)
			if (!refreshToken.RefreshToken.Equals(Request.Cookies["X-RefreshToken"].ToString()))
				return Unauthorized(ValidationMessages.Auth_InvalidRefreshToken);

		if (refreshToken.RefreshTokenExpires < DateTime.Now)
			return Unauthorized(ValidationMessages.Auth_ExpiredRefreshToken);
		#endregion

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _authRepository.UpdateRefreshToken(userInfo.Id, token);
		return Ok(token);
	});
}