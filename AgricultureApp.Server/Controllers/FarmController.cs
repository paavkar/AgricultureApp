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
                return Unauthorized();
            }

            FarmResult result = await farmService.CreateAsync(farmDto, userId);
            return !result.Succeeded
                ? BadRequest(result)
                : CreatedAtAction(nameof(CreateFarm), result);
        }
    }
}
