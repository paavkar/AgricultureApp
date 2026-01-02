using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AgricultureApp.MauiClient.Data
{
    public class UserRepository
    {
        private HttpClient _httpClient { get; set; }
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IHttpClientFactory httpClientFactory, ILogger<UserRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<UserResult> GetLoggedInUserAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/user/me");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri);
                UserResult result = await response.Content.ReadFromJsonAsync<UserResult>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to fetch user info: {Method}", nameof(GetLoggedInUserAsync));
                await AppShell.DisplaySnackbarAsync(ex.Message);
                return new UserResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }
    }
}
