using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
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

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = ["User authentication failed."]
                    });
            }

            FarmResult result = await farmService.CreateAsync(farmDto, userId);
            return !result.Succeeded
                ? BadRequest(result)
                : CreatedAtAction(nameof(CreateFarm), result);
        }

        [HttpPatch("update/{farmId}")]
        public async Task<IActionResult> UpdateFarm(string farmId, [FromBody] UpdateFarmDto farmDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
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

            FarmResult result = await farmService.UpdateAsync(farmDto, userId);
            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }
    }
}
