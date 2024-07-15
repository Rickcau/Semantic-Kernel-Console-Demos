

namespace Cosmos.Chat.GPT.Models
{
    public class ChatUser
    {
        public string? Id { get; set; }
        public string? DocumentType { get; set; }
        public string? ChatUserId { get; set; }
        public string? UserName { get; set; }
        public List<Session> Sessions { get; set; } = new List<Session>();

    }
}