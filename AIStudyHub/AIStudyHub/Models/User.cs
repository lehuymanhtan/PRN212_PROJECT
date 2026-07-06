using System;
using System.Collections.Generic;

namespace AIStudyHub.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relationship (Navigation Property)
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
