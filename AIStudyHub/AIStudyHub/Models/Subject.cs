using System;

namespace AIStudyHub.Models
{
    public class Subject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // Foreign Key
        public string Name { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? ColorHex { get; set; }

        // Relationship (Navigation Property)
        public User? User { get; set; }
    }
}
