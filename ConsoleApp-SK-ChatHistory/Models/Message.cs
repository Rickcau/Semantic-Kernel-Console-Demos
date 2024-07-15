namespace Cosmos.Chat.GPT.Models
{
    public class Message
    {
        public string? Id { get; set; }
        public string? DocumentType { get; set; }
        public string? MessageId { get; set; }
        public string? SessionId { get; set; }
        public string? ChatUserId { get; set; } // Add UserId as the partition key
        public string? Prompt { get; set; }
        public string? Completion { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public DateTime Timestamp { get; set; }
    }
}