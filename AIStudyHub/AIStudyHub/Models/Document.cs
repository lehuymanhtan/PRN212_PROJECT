using System;

namespace AIStudyHub.Models
{
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SubjectId { get; set; } // Foreign Key
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; } // "PDF", "DOCX", etc.
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Relationship (Navigation Property)
        public Subject? Subject { get; set; }
    }
}
