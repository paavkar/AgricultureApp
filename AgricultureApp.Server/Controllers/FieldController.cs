using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
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
    public class FieldController(IFarmService farmService,
        IStringLocalizer<AgricultureAppLoc> localizer) : ControllerBase
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
                        Errors = [localizer["UserAuthenticationFailed"]]
                    });
            }

            if (fieldDto.OwnerFarmId != farmId || fieldDto.FarmId != farmId ||
                fieldDto.FarmId != fieldDto.OwnerFarmId)
            {
                return BadRequest(new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FarmIdNotMatchingField"]]
                });
            }

            FieldResult result = await farmService.CreateFieldAsync(fieldDto, userId);

            if (!result.Succeeded)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result),
                };
            }

            return CreatedAtAction(nameof(AddFieldToFarm), result);
        }

        [HttpGet("get/{fieldId}")]
        public async Task<IActionResult> GetFieldById(string fieldId)
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

            FieldResult result = await farmService.GetFieldByIdAsync(fieldId, userId);

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
                        Errors = [localizer["UserAuthenticationFailed"]]
                    });
            }

            BaseResult result = await farmService.UpdateFieldCurrentFarmAsync(fieldId, update, userId);

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
                        Errors = [localizer["UserAuthenticationFailed"]]
                    });
            }

            BaseResult result = await farmService.RevertFieldCurrentFarmAsync(fieldId, update, userId);

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
                        Errors = [localizer["UserAuthenticationFailed"]]
                    });
            }

            if (update.FieldId != fieldId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["FieldIdNotMatchingURL"]]
                    });
            }

            BaseResult result = await farmService.UpdateFieldAsync(update, userId);

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

        [HttpPatch("update-status/{fieldId}")]
        public async Task<IActionResult> UpdateFieldStatus(string fieldId, [FromBody] UpdateFieldStatusDto fieldStatusDto)
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

            if (fieldStatusDto.FieldId != fieldId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["FieldIdNotMatchingURL"]]
                    });
            }

            BaseResult result = await farmService.UpdateFieldStatusAsync(fieldStatusDto, userId);

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

        [HttpPost("add-cultivation/{fieldId}")]
        public async Task<IActionResult> AddFieldCultivation(string fieldId, [FromBody] CreateFieldCultivationDto cultivationDto)
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

            if (cultivationDto.FieldId != fieldId)
            {
                return BadRequest(new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldIdNotMatchingURL"]]
                });
            }

            FieldCultivationResult result = await farmService.AddFieldCultivationAsync(cultivationDto, userId);

            if (!result.Succeeded)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => Forbid(),
                    _ => BadRequest(result),
                };
            }

            return CreatedAtAction(nameof(AddFieldCultivation), result);
        }

        [HttpGet("cultivations/{fieldId}")]
        public async Task<IActionResult> GetFieldCultivations(string fieldId, [FromBody] string farmId)
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

            FieldCultivationResult result = await farmService.GetFieldCultivationsAsync(fieldId, farmId, userId);

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

        [HttpPatch("set-harvested/{cultivationId}")]
        public async Task<IActionResult> SetFieldHarvested(string cultivationId, [FromBody] FieldHarvestDto harvestDto)
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

            if (harvestDto.FieldCultivationId != cultivationId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["CultivationIdNotMatchingURL"]]
                    });
            }

            BaseResult result = await farmService.SetFieldHarvestedAsync(harvestDto, userId);

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

        [HttpPatch("update-cultivation-status/{cultivationId}")]
        public async Task<IActionResult> UpdateFieldCultivationStatus(string cultivationId, [FromBody] UpdateFieldCultivationStatusDto update)
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

            if (update.FieldCultivationId != cultivationId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["CultivationIdNotMatchingURL"]]
                    });
            }

            BaseResult result = await farmService.UpdateFieldCultivationStatusAsync(update, userId);

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

        [HttpDelete("delete-cultivation/{cultivationId}")]
        public async Task<IActionResult> DeleteFieldCultivation(string cultivationId, [FromBody] DeleteFieldCultivationDto deleteItems)
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

            if (deleteItems.FieldCultivationId != cultivationId)
            {
                return BadRequest(
                    new BaseResult
                    {
                        Succeeded = false,
                        Errors = [localizer["CultivationIdNotMatchingURL"]]
                    });
            }

            BaseResult result = await farmService.DeleteFieldCultivationAsync(deleteItems, userId);

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
