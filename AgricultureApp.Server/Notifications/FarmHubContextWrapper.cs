using AgricultureApp.Application.Notifications;
using AgricultureApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AgricultureApp.Server.Notifications
{
    public class FarmHubContextWrapper(
        IHubContext<FarmHub> hubContext,
        ILogger<FarmHubContextWrapper> logger) : IFarmHubContext
    {
        public async Task SendToUserAsync(string userId, string method, string arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.Clients.User(userId)
                    .SendAsync(method, arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message to user {UserId} via method {Method}.", userId, method);
            }
        }

        public async Task SendToGroupAsync(string groupName, string method, object? arg, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.Clients.Group(groupName)
                    .SendAsync(method, arg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message to group {GroupName} via method {Method}.", groupName, method);
            }
        }
    }
}
