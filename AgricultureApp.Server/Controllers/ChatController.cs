using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.LLM;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgricultureApp.Server.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class ChatController(ILlmService llmService, ILogger<ChatController> logger) : ControllerBase
    {
        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat()
        {
            var chatId = await llmService.CreateChatHistoryAsync();

            return Ok(new { ChatId = chatId });
        }

        [HttpPost("send-message/{chatId}")]
        public async Task<IActionResult> SendMessage(string chatId, [FromBody] MessageDto message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await llmService.GenerateStreamingResponseAsync(chatId, message.Content, message.FarmId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to generate streaming response for chat {ChatId}.", chatId);
                }
            });

            return Accepted();
        }
    }
}
