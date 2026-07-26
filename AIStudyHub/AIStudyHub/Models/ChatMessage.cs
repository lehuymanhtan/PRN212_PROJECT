using System;

namespace AIStudyHub.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? DocumentId { get; set; }
        
        /// <summary>
        /// Role có thể là "user" hoặc "model"
        /// </summary>
        public string Role { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Document? Document { get; set; }
    }
}
