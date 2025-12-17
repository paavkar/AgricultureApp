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
    public class FieldController(IFarmService farmService) : ControllerBase
    {
        [HttpPost("add/{farmId}")]
        public async Task<IActionResult> AddFieldToFarm(string farmId, [FromBody] CreateFieldDto fieldDto)
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

            if (fieldDto.OwnerFarmId != farmId || fieldDto.FarmId != farmId ||
                fieldDto.FarmId != fieldDto.OwnerFarmId)
            {
                return BadRequest(new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Farm ID must match in the URL and in the body."]
                });
            }

            FieldResult result = await farmService.CreateFieldAsync(fieldDto, userId);

            return !result.Succeeded
                ? BadRequest(result)
                : CreatedAtAction(nameof(AddFieldToFarm), result);
        }

        [HttpPatch("update-farm/{fieldId}")]
        public async Task<IActionResult> UpdateFieldCurrentFarm(string fieldId, [FromBody] UpdateFieldFarmDto update)
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

            BaseResult result = await farmService.UpdateFieldCurrentFarmAsync(fieldId, update, userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpPatch("revert-management/{fieldId}")]
        public async Task<IActionResult> RevertFieldCurrentFarm(string fieldId, [FromBody] UpdateFieldFarmDto update)
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

            BaseResult result = await farmService.RevertFieldCurrentFarmAsync(fieldId, update, userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpPatch("update/{fieldId}")]
        public async Task<IActionResult> Update(string fieldId, [FromBody] UpdateFieldDto update)
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

            if (update.FieldId != fieldId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = ["Field ID doesn't match the one in the URL."]
                    });
            }

            BaseResult result = await farmService.UpdateFieldAsync(update, userId);

            return !result.Succeeded
                ? BadRequest(result)
                : Ok(result);
        }
    }
}
