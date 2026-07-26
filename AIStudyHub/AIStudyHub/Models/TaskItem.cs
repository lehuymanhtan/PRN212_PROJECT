using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIStudyHub.Models
{
    /// <summary>
    /// Đại diện cho một Task/Deadline/Lịch thi trong hệ thống.
    /// Tên class là TaskItem để tránh xung đột với System.Threading.Tasks.Task.
    /// </summary>
    public class TaskItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubjectId { get; set; } // Foreign Key

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Trạng thái công việc: Todo, InProgress, Done
        /// </summary>
        public string Status { get; set; } = DeadlineStatus.Todo;

        /// <summary>
        /// Loại công việc: Assignment, Exam, Review
        /// </summary>
        public string Type { get; set; } = DeadlineType.Assignment;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey(nameof(SubjectId))]
        public Subject? Subject { get; set; }
    }

    /// <summary>
    /// Hằng số trạng thái Task - đặt tên DeadlineStatus để tránh xung đột
    /// với System.Threading.Tasks.TaskStatus.
    /// </summary>
    public static class DeadlineStatus
    {
        public const string Todo = "Todo";
        public const string InProgress = "InProgress";
        public const string Done = "Done";
    }

    /// <summary>
    /// Hằng số loại Task.
    /// </summary>
    public static class DeadlineType
    {
        public const string Assignment = "Assignment";
        public const string Exam = "Exam";
        public const string Review = "Review";
    }
}
