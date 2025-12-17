using AgricultureApp.Application.Auth;
using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.SharedKernel.Localization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
            return !result.Succeeded ? BadRequest(result) : Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var platform = Request.Headers["X-Client-Platform"].ToString();

            AuthResult result = await authService.LoginAsync(loginDto, platform);
            return !result.Succeeded ? Unauthorized(result) : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            AuthResult result = await authService.RefreshTokenAsync(refreshToken);
            return !result.Succeeded ? BadRequest(result) : Ok(result);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var success = await authService.RevokeRefreshTokenAsync(refreshToken);
            return !success
                ? BadRequest(new { Message = localizer["InvalidRefresh"] })
                : Ok(new { Message = localizer["RefreshRevoked"] });
        }
    }
}
