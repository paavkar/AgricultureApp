using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace AgricultureApp.Server.Hubs
{
    public class SubUserProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }
    }
}
