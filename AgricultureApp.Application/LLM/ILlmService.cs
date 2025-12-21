namespace AgricultureApp.Application.LLM
{
    public interface ILlmService
    {
        Task<string> CreateChatHistoryAsync();
        Task GenerateStreamingResponseAsync(string chatId, string message, string farmId);
    }
}
