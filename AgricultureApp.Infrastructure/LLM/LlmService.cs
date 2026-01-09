using AgricultureApp.Application.Farms;
using AgricultureApp.Application.LLM;
using AgricultureApp.Application.Notifications;
using AgricultureApp.SharedKernel.Localization;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace AgricultureApp.Infrastructure.LLM
{
    public class LlmService : ILlmService
    {
        private Kernel Kernel;
        private HybridCache Cache;
        private ILogger<LlmService> Logger;
        private IChatCompletionService ChatCompletionService;
        private IFarmNotificationService NotificationService;
        private IStringLocalizer<AgricultureAppLoc> Localizer;
        OpenAIPromptExecutionSettings OpenAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        string SystemPrompt = """
            Use plain text only. Do not use any markdown, HTML, or other formatting.
            You will be given a farm id before the user message like this: Farm ID: <GUID>. <USER_MESSAGE>.
            DO NOT USE THE ID OF THE FARM OR FIELDS IN YOUR RESPONSE.
            """;

        public LlmService(
            IConfiguration configuration,
            HybridCache cache,
            IFarmNotificationService notificationService,
            ILogger<LlmService> logger,
            IStringLocalizer<AgricultureAppLoc> localizer,
            IFarmRepository farmRepository)
        {
            var modelId = configuration["LLM:ModelId"] ?? throw new InvalidOperationException("LLM:ModelId not found.");
            var endpoint = configuration["LLM:Endpoint"] ?? throw new InvalidOperationException("LLM:Endpoint not found."); ;
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                kernelBuilder.AddOllamaChatCompletion(
                    modelId: modelId,
                    endpoint: new Uri(endpoint));
                OpenAIPromptExecutionSettings.FunctionChoiceBehavior =
                    FunctionChoiceBehavior.Auto(autoInvoke: false);
            }
            else
            {
                modelId = configuration["LLM:AzureModelId"] ?? throw new InvalidOperationException("LLM:AzureModelId not found.");
                endpoint = configuration["LLM:AzureEndpoint"] ?? throw new InvalidOperationException("LLM:AzureEndpoint not found.");
                var deploymentName = configuration["LLM:DeploymentName"]
                    ?? throw new InvalidOperationException("LLM:DeploymentName not found.");
                var apiKey = configuration["LLM:ApiKey"]
                    ?? throw new InvalidOperationException("LLM:ApiKey not found.");

                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName: deploymentName,
                    apiKey: apiKey,
                    endpoint: endpoint,
                    modelId: modelId);
            }
            kernelBuilder.Plugins.AddFromObject(
                new FarmPlugin(farmRepository),
                "FarmPlugin");
            Kernel = kernelBuilder.Build();
            ChatCompletionService = Kernel.GetRequiredService<IChatCompletionService>();

            Cache = cache;
            NotificationService = notificationService;
            Logger = logger;
            Localizer = localizer;
        }

        public async Task<string> CreateChatHistoryAsync()
        {
            var chatId = Guid.CreateVersion7().ToString();

            HybridCacheEntryOptions cacheEntryOptions = new()
            {
                Expiration = TimeSpan.FromHours(1)
            };

            ChatHistory history = [];
            history.AddSystemMessage(SystemPrompt);
            history.AddAssistantMessage(Localizer["FirstAssistantMessage"]);

            await Cache.SetAsync(chatId, history, cacheEntryOptions);

            return chatId;
        }

        public async Task GenerateStreamingResponseAsync(string chatId, string message, string farmId)
        {
            ChatHistory chatHistory = await Cache.GetOrCreateAsync(chatId, async entry =>
            {
                ChatHistory history = [];
                history.AddSystemMessage(SystemPrompt);
                return history;
            });

            chatHistory.AddUserMessage($"{Localizer["FarmId", farmId]}. {message}");

            StringBuilder assistantResponse = new();
            var responseId = Guid.CreateVersion7().ToString();

            await foreach (StreamingChatMessageContent token in
                ChatCompletionService.GetStreamingChatMessageContentsAsync(
                    chatHistory,
                    executionSettings: OpenAIPromptExecutionSettings,
                    kernel: Kernel))
            {
                try
                {
                    assistantResponse.Append(token.Content);
                    await NotificationService.NotifyLlmStreamingResponseAsync(chatId,
                        new
                        {
                            Content = token.Content,
                            AuthorName = token.AuthorName,
                            AuthorRole = token.Role,
                            MessageId = responseId
                        });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to send streaming token for chat {ChatId}.", chatId);
                }
            }

            chatHistory.AddAssistantMessage(assistantResponse.ToString());
            await Cache.SetAsync(chatId, chatHistory, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            });

            try
            {
                await NotificationService.NotifyLlmStreamingFinishedAsync(chatId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send streaming finished notification for chat {ChatId}.", chatId);
            }
        }
    }
}
