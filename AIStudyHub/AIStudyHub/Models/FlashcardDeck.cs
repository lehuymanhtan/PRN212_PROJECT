using System;
using System.ComponentModel.DataAnnotations;

namespace AIStudyHub.Models
{
    public class FlashcardDeck
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string SubjectId { get; set; }

        public string? DocumentId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
    }
}
