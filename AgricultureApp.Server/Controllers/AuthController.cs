using AgricultureApp.Application.Auth;
using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Infrastructure.Auth;
using AgricultureApp.SharedKernel.Localization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace AgricultureApp.Server.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthController(IAuthService authService,
        IStringLocalizer<AgricultureAppLoc> localizer) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();

            AuthResult result = await authService.RegisterAsync(registerDto, platform);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            if (platform.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(7),
                    Path = "/"
                };
                Response.Cookies.Append("refresh_token", result.RefreshToken!, cookieOptions);
                result.RefreshToken = null; // Do not return refresh token in response body for web clients
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            AuthResult result = await authService.LoginAsync(loginDto, platform);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            if (platform.Equals("web", StringComparison.OrdinalIgnoreCase) && !result.TwoFactorRequired)
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(7),
                    Path = "/"
                };
                Response.Cookies.Append("refresh_token", result.RefreshToken!, cookieOptions);
                result.RefreshToken = null; // Do not return refresh token in response body for web clients
            }

            return Ok(result);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto twoFactorDto)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();
            AuthResult result = await authService.VerifyTwoFactorAsync(twoFactorDto, platform);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            if (platform.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(7),
                    Path = "/"
                };
                Response.Cookies.Append("refresh_token", result.RefreshToken!, cookieOptions);
                result.RefreshToken = null; // Do not return refresh token in response body for web clients
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("setup-2fa")]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            AuthResult result = await authService.SetupTwoFactorAsync(userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [Authorize]
        [HttpGet("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] VerifyTwoFactorDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            Console.WriteLine(model.Code);

            AuthResult result = await authService.EnableTwoFactorAsync(userId, model);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [Authorize]
        [HttpPost("disable-2fa")]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            AuthResult result = await authService.DisableTwoFactorAsync(userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] JwtRefreshRequest? request)
        {
            var refreshToken = request?.RefreshToken ?? "";
            var platform = Request.Headers["X-Client-Platform"].ToString();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = Request.Cookies["refresh_token"];
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new BaseResult { Succeeded = false, Errors = [localizer["RefreshTokenMissing"]] });
            }

            AuthResult result = await authService.RefreshTokenAsync(refreshToken);
            if (!result.Succeeded)
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };
                Response.Cookies.Delete("refresh_token", cookieOptions);
                Response.Cookies.Delete("access_token", cookieOptions);
                return BadRequest(result);
            }

            if (platform.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(7),
                    Path = "/"
                };
                Response.Cookies.Append("refresh_token", result.RefreshToken!, cookieOptions);
                cookieOptions.MaxAge = TimeSpan.FromHours(1);
                Response.Cookies.Append("access_token", result.AccessToken!, cookieOptions);
                result.RefreshToken = null; // Do not return refresh token in response body for web clients
                result.AccessToken = null; // Do not return access token in response body for web clients
            }

            return Ok(result);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] JwtRefreshRequest? request)
        {
            var refreshToken = request?.RefreshToken ?? "";
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = Request.Cookies["refresh_token"];
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new BaseResult { Succeeded = false, Errors = [localizer["RefreshTokenMissing"]] });
            }
            var success = await authService.RevokeRefreshTokenAsync(refreshToken);

            if (!success)
            {
                return BadRequest(new BaseResult { Succeeded = false, Errors = [localizer["InvalidRefresh"]] });
            }

            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            Response.Cookies.Delete("refresh_token", cookieOptions);
            Response.Cookies.Delete("access_token", cookieOptions);

            return Ok(new BaseResult { Succeeded = true, Message = localizer["RefreshRevoked"] });
        }
    }
}
