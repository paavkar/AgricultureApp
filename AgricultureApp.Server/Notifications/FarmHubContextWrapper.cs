using AgricultureApp.Application.Notifications;
using AgricultureApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AgricultureApp.Server.Notifications
{
    public class FarmHubContextWrapper(IHubContext<FarmHub> hubContext) : IFarmHubContext
    {
        public async Task SendToUserAsync(string userId, string method, string arg, CancellationToken cancellationToken = default)
        {
            await hubContext.Clients.User(userId)
                .SendAsync(method, arg, cancellationToken);
        }
        public async Task SendToGroupAsync(string groupName, string method, object? arg, CancellationToken cancellationToken = default)
        {
            await hubContext.Clients.Group(groupName)
                .SendAsync(method, arg, cancellationToken);
        }
    }
}
