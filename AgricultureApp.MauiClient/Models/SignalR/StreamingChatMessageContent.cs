namespace AgricultureApp.MauiClient.Models
{
    public class StreamingChatMessageContent
    {
        public string Content { get; set; }
        public string? AuthorName { get; set; }
        public AuthorRole? Role { get; set; }
    }

    public struct AuthorRole
    {
        public string Label { get; set; }
    }
}
