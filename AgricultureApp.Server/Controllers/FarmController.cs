using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
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
    public class FarmController(
        IFarmService farmService,
        IStringLocalizer<AgricultureAppLoc> localizer) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateFarm([FromBody] CreateFarmDto farmDto)
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

            FarmResult<Farm> result = await farmService.CreateAsync(farmDto, userId);
            return !result.Succeeded
                ? BadRequest(result)
                : CreatedAtAction(nameof(CreateFarm), result);
        }

        [HttpGet("full-info/{farmId}")]
        public async Task<IActionResult> GetFullFarmInfo(string farmId)
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

            IEnumerable<string> userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            FarmResult<FarmDto> result = await farmService.GetFullInfoAsync(farmId);

            if (!result.Succeeded)
            {
                return result.StatusCode == 404
                    ? NotFound(result)
                    : !userRoles.Contains("Admin") && result.Farm!.OwnerId != userId
                    && result.Farm.Managers.All(m => m.UserId != userId)
                    ? Forbid()
                    : BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("get-owned")]
        public async Task<IActionResult> GetOwnedFarms()
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

            FarmResult<FarmDto> result = await farmService.GetByOwnerAsync(userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpGet("get-managed")]
        public async Task<IActionResult> GetManagedFarms()
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

            FarmResult<FarmDto> result = await farmService.GetByManagerAsync(userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpPatch("update/{farmId}")]
        public async Task<IActionResult> UpdateFarm(string farmId, [FromBody] UpdateFarmDto farmDto)
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

            if (farmDto.OwnerId != userId)
            {
                return Forbid();
            }

            if (farmDto.Id != farmId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["FarmIdNotMatchingURL"]]
                    });
            }

            FarmResult<Farm> result = await farmService.UpdateAsync(farmDto, userId);
            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpDelete("delete/{farmId}")]
        public async Task<IActionResult> DeleteFarm(string farmId)
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

            BaseResult result = await farmService.DeleteAsync(farmId, userId);

            return !result.Succeeded
                ? BadRequest(result)
                : NoContent();
        }

        [HttpPost("add-manager/{farmId}")]
        public async Task<IActionResult> AddFarmManager(string farmId, [FromBody] string email)
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

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["EmailMustBeProvided"]]
                });
            }

            ManagerResult result = await farmService.AddManagerAsync(userId, farmId, email);

            if (!result.Succeeded)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result),
                };
            }

            return Ok(result);
        }

        [HttpDelete("remove-manager/{farmId}")]
        public async Task<IActionResult> RemoveFarmManager(string farmId, [FromBody] string managerId)
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

            if (string.IsNullOrWhiteSpace(managerId))
            {
                return BadRequest(new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["ManagerIdMustBeProvided"]]
                });
            }

            BaseResult result = await farmService.DeleteManagerAsync(farmId, userId, managerId);

            if (!result.Succeeded)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result),
                };
            }

            return NoContent();
        }
    }
}
