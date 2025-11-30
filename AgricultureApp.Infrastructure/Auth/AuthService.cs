using AgricultureApp.Application.Auth;
using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgricultureApp.Infrastructure.Auth
{
    public class AuthService(HybridCache cache, UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager) : IAuthService
    {
        private const int RefreshTokenExpiryDays = 7;

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto, string platform)
        {
            ApplicationUser user = new()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Name = registerDto.Name
            };

            IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            var userRoleExists = await roleManager.RoleExistsAsync("User");
            if (!userRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            await userManager.AddToRoleAsync(user, "User");

            platform = string.IsNullOrWhiteSpace(platform) ? "default" : platform;
            return await GenerateAuthResultAsync(user, platform);
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto, string platform)
        {
            ApplicationUser? user = loginDto.EmailOrUsername.Contains('@')
                ? await userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                : await userManager.FindByNameAsync(loginDto.EmailOrUsername);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = ["Invalid login attempt."]
                };
            }

            platform = string.IsNullOrWhiteSpace(platform) ? "default" : platform;
            return await GenerateAuthResultAsync(user, platform);
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            RefreshTokenInfo tokenInfo = await cache.GetOrCreateAsync<RefreshTokenInfo>(refreshToken, async entry => null);
            if (tokenInfo == null || tokenInfo.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = ["Invalid or expired refresh token."]
                };
            }

            ApplicationUser? user = await userManager.FindByIdAsync(tokenInfo.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = ["User not found."]
                };
            }

            // This is to ensure the old refresh token cannot be used
            await cache.RemoveAsync(refreshToken);
            return await GenerateAuthResultAsync(user, tokenInfo.Platform);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            await cache.RemoveAsync(refreshToken);
            return true;
        }

        private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user, string platform)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]!);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
            ];

            IList<string> roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"]
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            RefreshTokenInfo refreshTokenInfo = new()
            {
                Id = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpiryDays),
                Platform = platform
            };
            await cache.SetAsync(refreshToken, refreshTokenInfo, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(RefreshTokenExpiryDays)
            });

            return new AuthResult
            {
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
