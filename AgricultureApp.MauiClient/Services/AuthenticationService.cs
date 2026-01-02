using Microsoft.Extensions.Logging;
using System.Net;
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
        private readonly PlatformInfoService _platformInfoService;

        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            PlatformInfoService platformInfoService)
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
            _platformInfoService = platformInfoService;
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

        public async Task AddLanguageHeaders()
        {
            var locale = await SecureStorage.GetAsync("preferred_culture");
            _http.DefaultRequestHeaders.AcceptLanguage.Clear();
            _http.DefaultRequestHeaders.AcceptLanguage.Add(
                new StringWithQualityHeaderValue(locale ?? "en-GB"));
        }

        public async Task AddAuthHeader()
        {
            var accessToken = await GetAccessTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public void AddPlatfromHeader()
        {
            var platform = _platformInfoService.GetPlatformInfo();
            _http.DefaultRequestHeaders.Remove("X-Client-Platform");
            _http.DefaultRequestHeaders.Add(
                "X-Client-Platform", platform);
        }

        public async Task<bool> RefreshTokensAsync()
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            await AddLanguageHeaders();
            AddPlatfromHeader();

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

            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            return false;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/login");

            await AddLanguageHeaders();
            AddPlatfromHeader();

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

        public async Task<BaseResult> LogOutAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/revoke");
            var token = await GetRefreshTokenAsync();
            JwtRefreshRequest jwt = new()
            {
                RefreshToken = token
            };

            await AddLanguageHeaders();
            AddPlatfromHeader();

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(uri, jwt);
                BaseResult result = await response.Content.ReadFromJsonAsync<BaseResult>();

                if (result.Succeeded)
                {
                    SecureStorage.Remove(AccessTokenKey);
                    SecureStorage.Remove(RefreshTokenKey);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error in logout method: {Method}", nameof(LogOutAsync));
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<AuthResult> VerifyTwoFactorAsync(TwoFactorDto twoFactorDto)
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/verify-2fa");

            await AddLanguageHeaders();
            AddPlatfromHeader();

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(uri, twoFactorDto);
                AuthResult result = await response.Content.ReadFromJsonAsync<AuthResult>();

                if (response.IsSuccessStatusCode)
                {
                    await SaveTokensAsync(result.AccessToken, result.RefreshToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Method}", nameof(VerifyTwoFactorAsync));
                return new AuthResult { Succeeded = false, Errors = [ex.Message] };
            }
        }

        public async Task<AuthResult> SetupTwoFactorAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/setup-2fa");

            await AddLanguageHeaders();
            await AddAuthHeader();

            try
            {
                HttpResponseMessage response = await _http.GetAsync(uri);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await RefreshTokensAsync();

                    if (!refreshed)
                    {
                        AppShell.Current.Window.Page = new AuthShell();

                        return new AuthResult
                        {
                            Succeeded = false,
                            Errors = ["Unauthorized and token refresh failed"]
                        };
                    }

                    await AddAuthHeader();
                    response = await _http.GetAsync(uri);
                }

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.OK)
                {
                    AuthResult result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result;
                }

                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [""]
                };

            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<AuthResult> EnableTwoFactorAsync(VerifyTwoFactorDto dto)
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/enable-2fa");

            await AddLanguageHeaders();
            await AddAuthHeader();

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(uri, dto);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await RefreshTokensAsync();

                    if (!refreshed)
                    {
                        AppShell.Current.Window.Page = new AuthShell();

                        return new AuthResult
                        {
                            Succeeded = false,
                            Errors = ["Unauthorized and token refresh failed"]
                        };
                    }

                    await AddAuthHeader();
                    response = await _http.PostAsJsonAsync(uri, dto);
                }

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.OK)
                {
                    AuthResult result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result;
                }

                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [""]
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<AuthResult> DisableTwoFactorAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/auth/disable-2fa");

            await AddLanguageHeaders();
            await AddAuthHeader();

            try
            {
                HttpResponseMessage response = await _http.PostAsync(uri, null);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await RefreshTokensAsync();

                    if (!refreshed)
                    {
                        AppShell.Current.Window.Page = new AuthShell();

                        return new AuthResult
                        {
                            Succeeded = false,
                            Errors = ["Unauthorized and token refresh failed"]
                        };
                    }

                    await AddAuthHeader();
                    response = await _http.PostAsync(uri, null);
                }

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.OK)
                {
                    AuthResult result = await response.Content.ReadFromJsonAsync<AuthResult>();
                    return result;
                }

                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [""]
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }
    }
}
