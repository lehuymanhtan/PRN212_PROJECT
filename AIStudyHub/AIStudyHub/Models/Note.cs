using System;
using System.ComponentModel.DataAnnotations;

namespace AIStudyHub.Models
{
    public class Note
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Title { get; set; } = "Untitled Note";

        public string Content { get; set; } = string.Empty;

        // Optional link to a Subject, similar to FlashcardDeck
        public Guid? SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
