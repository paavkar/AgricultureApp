using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;

namespace AgricultureApp.Application.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto registerDto, string platform);
        Task<AuthResult> LoginAsync(LoginDto loginDto, string platform);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
