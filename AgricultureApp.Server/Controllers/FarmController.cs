using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgricultureApp.Server.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class FarmController(IFarmService farmService) : ControllerBase
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
                        Errors = ["User authentication failed."]
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
                        Errors = ["User authentication failed."]
                    });
            }
            IEnumerable<string> userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            FarmResult<FarmDto> result = await farmService.GetFullInfoAsync(farmId);

            return !result.Succeeded
                ? BadRequest(result)
                : !userRoles.Contains("Admin") && result.Farm!.OwnerId != userId
                    && result.Farm.Managers.All(m => m.UserId != userId)
                ? Forbid()
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
                        Errors = ["User authentication failed."]
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
                        Errors = ["Farm ID in the URL does not match the ID in the body."]
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
                        Errors = ["User authentication failed."]
                    });
            }

            BaseResult result = await farmService.DeleteAsync(farmId, userId);


            return !result.Succeeded
                ? BadRequest(result)
                : NoContent();
        }
    }
}
