using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AgricultureApp.Server.Hubs
{
    public class SubUserProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
