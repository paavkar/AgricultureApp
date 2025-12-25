using AgricultureApp.Application.Farms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgricultureApp.Server.Hubs
{
    [Authorize]
    public class FarmHub(
        IFarmRepository farmRepository,
        ILogger<FarmHub> logger) : Hub
    {
        public async Task JoinFarm(string farmId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Unauthorized user attempted to join farm hub.");
            }

            var isManager = await farmRepository.IsUserFarmManagerAsync(farmId, userId);

            if (!isManager)
            {
                logger.LogWarning("User {UserId} is not a manager of farm {FarmId} and cannot join the hub.", userId, farmId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, farmId);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
