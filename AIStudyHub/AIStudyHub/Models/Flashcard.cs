using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIStudyHub.Models
{
    public class Flashcard
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string DeckId { get; set; }

        [Required]
        public string FrontText { get; set; }

        [Required]
        public string BackText { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public int RepetitionCount { get; set; } = 0;

        // The following fields are typically used for the SuperMemo-2 (SM-2) algorithm
        // which Anki is based on. 
        // EaseFactor usually starts at 2.5
        public double EaseFactor { get; set; } = 2.5;

        // Interval in days for the next review
        public int Interval { get; set; } = 0;
    }
}
