using System.Text.Json.Serialization;
namespace Cosmos.Chat.GPT.Models
{
    public class Session
    {
        public string? Id { get; set; }
        public string? DocumentType { get; set; }
        public string? SessionId { get; set; }

        public string? Name { get; set; }
        public string? ChatUserId { get; set; }
        public DateTime StartTime { get; set; }
        
        [JsonIgnore]  // When we serialize the session object we don't want to include all the messages as the payload will be large, retreive those messages when needed
        public List<String> Messages { get; set; } = new List<String>();
    }
}
