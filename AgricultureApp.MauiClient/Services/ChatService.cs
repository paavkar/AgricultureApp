using AgricultureApp.MauiClient.Models.Chat;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AgricultureApp.MauiClient.Services
{
    public class ChatService
    {
        private HttpClient _httpClient { get; set; }
        private readonly ILogger<ChatService> _logger;

        public ChatService(IHttpClientFactory httpClientFactory, ILogger<ChatService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<string> CreateChatAsync()
        {
            Uri uri = new(Constants.ApiBaseUrl + "v1/chat/create-chat");
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(uri, null);

                CreatChatResponse result = await response.Content.ReadFromJsonAsync<CreatChatResponse>();

                return response.IsSuccessStatusCode
                    ? result.ChatId
                    : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> SendMessageAsync(Message message, string chatId)
        {
            Uri uri = new(Constants.ApiBaseUrl + $"v1/chat/send-message/{chatId}");
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uri, message);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public record CreatChatResponse(string ChatId);
}
