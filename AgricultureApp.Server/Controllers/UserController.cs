using AgricultureApp.Application.ResultModels;
using AgricultureApp.Application.Users;
using AgricultureApp.SharedKernel.Localization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace AgricultureApp.Server.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class UserController(
        IUserService userService,
        IStringLocalizer<AgricultureAppLoc> localizer) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetLoggedInUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["UserAuthenticationFailed"]]
                    });
            }

            UserResult result = await userService.GetByIdAsync(userId);

            return !result.Succeeded
                ? result.StatusCode == 404
                    ? NotFound(result)
                    : BadRequest(result)
                : Ok(result);
        }
    }
}
