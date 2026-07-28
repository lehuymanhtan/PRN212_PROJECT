using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIStudyHub.Models
{
    /// <summary>
    /// Đại diện cho một Task/Deadline/Lịch thi trong hệ thống.
    /// Tên class là TaskItem để tránh xung đột với System.Threading.Tasks.Task.
    /// Kế thừa ObservableValidator để hỗ trợ cả [ObservableProperty] và Validation Attributes như [Required].
    /// </summary>
    public partial class TaskItem : ObservableValidator
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private Guid _subjectId;

        [Required]
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime? _dueDate;

        /// <summary>
        /// Trạng thái công việc: Todo, InProgress, Done
        /// </summary>
        [ObservableProperty]
        private string _status = DeadlineStatus.Todo;

        /// <summary>
        /// Loại công việc: Assignment, Exam, Review
        /// </summary>
        [ObservableProperty]
        private string _type = DeadlineType.Assignment;

        [ObservableProperty]
        private DateTime _createdAt = DateTime.Now;

        // Navigation property
        [ForeignKey(nameof(SubjectId))]
        [ObservableProperty]
        private Subject? _subject;
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

    /// <summary>
    /// Ánh xạ trạng thái Task giữa Tiếng Việt (UI) và Tiếng Anh (DB).
    /// </summary>
    public static class StatusMapper
    {
        public const string All = "Tất cả";
        public const string Todo = "Chưa làm";
        public const string InProgress = "Đang làm";
        public const string Done = "Hoàn thành";

        public static string ToVietnamese(string? status) => status switch
        {
            DeadlineStatus.Todo or "Chưa làm" => Todo,
            DeadlineStatus.InProgress or "Đang làm" => InProgress,
            DeadlineStatus.Done or "Hoàn thành" => Done,
            _ => Todo
        };

        public static string ToDbValue(string? status) => status switch
        {
            Todo or "Todo" => DeadlineStatus.Todo,
            InProgress or "InProgress" => DeadlineStatus.InProgress,
            Done or "Done" => DeadlineStatus.Done,
            _ => DeadlineStatus.Todo
        };
    }

    /// <summary>
    /// Ánh xạ loại Task giữa Tiếng Việt (UI) và Tiếng Anh (DB).
    /// </summary>
    public static class TypeMapper
    {
        public const string All = "Tất cả";
        public const string Assignment = "Bài tập";
        public const string Exam = "Lịch thi";
        public const string Review = "Ôn tập";

        public static string ToVietnamese(string? type) => type switch
        {
            DeadlineType.Assignment or "Bài tập" => Assignment,
            DeadlineType.Exam or "Lịch thi" => Exam,
            DeadlineType.Review or "Ôn tập" => Review,
            _ => Assignment
        };

        public static string ToDbValue(string? type) => type switch
        {
            Assignment or "Assignment" => DeadlineType.Assignment,
            Exam or "Exam" => DeadlineType.Exam,
            Review or "Review" => DeadlineType.Review,
            _ => DeadlineType.Assignment
        };
    }
}
