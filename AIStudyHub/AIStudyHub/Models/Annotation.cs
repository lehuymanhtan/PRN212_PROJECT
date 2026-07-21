using System;
using System.ComponentModel.DataAnnotations;

namespace AIStudyHub.Models
{
    public class Annotation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string DocumentId { get; set; }

        public int PageNumber { get; set; }

        public double PosX { get; set; }

        public double PosY { get; set; }

        public string? Content { get; set; }

        public string Type { get; set; } = "Highlight"; // e.g., Highlight, Note
    }
}
