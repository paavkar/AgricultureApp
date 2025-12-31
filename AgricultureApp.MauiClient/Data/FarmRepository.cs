using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AgricultureApp.MauiClient.Data
{
    public class FarmRepository
    {
        private HttpClient _httpClient { get; set; }
        private readonly ILogger<FarmRepository> _logger;

        public FarmRepository(IHttpClientFactory httpClientFactory, ILogger<FarmRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<FarmResult> GetOwnedFarmsAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/Farm/get-owned");
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            try
            {
                FarmResult result = await response.Content.ReadFromJsonAsync<FarmResult>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in {Method}", nameof(GetOwnedFarmsAsync));
                await AppShell.DisplaySnackbarAsync(ex.Message);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<FarmResult> GetManagedFarmsAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/Farm/get-managed");
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            try
            {
                FarmResult result = await response.Content.ReadFromJsonAsync<FarmResult>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in {Method}", nameof(GetManagedFarmsAsync));
                await AppShell.DisplaySnackbarAsync(ex.Message);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<FarmResult> GetFarmAsync(string id)
        {
            Uri uri = new(Constants.ApiBaseUrl + $"v1/Farm/full-info/{id}");
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            try
            {
                FarmResult result = await response.Content.ReadFromJsonAsync<FarmResult>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in {Method}", nameof(GetFarmAsync));
                await AppShell.DisplaySnackbarAsync(ex.Message);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
        }
    }
}
