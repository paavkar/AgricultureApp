using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AgricultureApp.MauiClient.Services
{
    public class AuthenticationService
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";

        private readonly HttpClient _http;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(ILogger<AuthenticationService> logger)
        {
            HttpClientHandler handler = new();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return cert != null && cert.Issuer.Equals("CN=localhost")
                    ? true
                    : errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif
            _http = new HttpClient(handler);
            _logger = logger;
        }

        public async Task<string?> GetAccessTokenAsync()
            => await SecureStorage.GetAsync(AccessTokenKey);

        public async Task<string?> GetRefreshTokenAsync()
            => await SecureStorage.GetAsync(RefreshTokenKey);

        public async Task SaveTokensAsync(string accessToken, string refreshToken)
        {
            await SecureStorage.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        public async Task AddHeaders()
        {
            var locale = await SecureStorage.GetAsync("preferred_culture");
            _http.DefaultRequestHeaders.AcceptLanguage.Clear();
            _http.DefaultRequestHeaders.AcceptLanguage.Add(
                new StringWithQualityHeaderValue(locale ?? "en-GB"));
        }

        public async Task<bool> RefreshTokensAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            await AddHeaders();

            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/refresh");
            HttpResponseMessage response = await _http.PostAsJsonAsync(uri,
                new JwtRefreshRequest
                {
                    RefreshToken = refreshToken
                });

            if (!response.IsSuccessStatusCode)
                return false;

            AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();
            if (result.Succeeded)
            {
                await SaveTokensAsync(result!.AccessToken!, result.RefreshToken!);
                return true;
            }

            await SecureStorage.SetAsync(AccessTokenKey, string.Empty);
            await SecureStorage.SetAsync(RefreshTokenKey, string.Empty);
            return false;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/login");

            await AddHeaders();

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(uri, loginDto);
                AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();

                if (response.IsSuccessStatusCode)
                {
                    if (result.TwoFactorRequired)
                        return result;
                    await SaveTokensAsync(result!.AccessToken!, result.RefreshToken!);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login method: {Method}", nameof(LoginAsync));
                return new AuthResult { Succeeded = false, Errors = [ex.Message] };
            }
        }

        public async Task<AuthResult> VerifyTwoFactorAsync(TwoFactorDto twoFactorDto)
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/verify-2fa");

            await AddHeaders();

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(uri, twoFactorDto);
                AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();

                if (response.IsSuccessStatusCode)
                {
                    await SaveTokensAsync(result!.AccessToken!, result.RefreshToken!);
                }

                return result;
            }
            catch (Exception)
            {
                return new AuthResult { Succeeded = false };
            }
        }
    }
}
