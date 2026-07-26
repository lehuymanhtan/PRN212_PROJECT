using System;
using System.Linq;
using System.Windows.Media;
using AIStudyHub.Converters;
using AIStudyHub.Models;
using AIStudyHub.ViewModels;
using Xunit;

namespace AIStudyHub.Tests
{
    public class TaskViewModelTests
    {
        [Fact]
        public void StatusMapper_BidirectionalMapping_WorksCorrectly()
        {
            Assert.Equal(StatusMapper.Todo, StatusMapper.ToVietnamese(DeadlineStatus.Todo));
            Assert.Equal(StatusMapper.InProgress, StatusMapper.ToVietnamese(DeadlineStatus.InProgress));
            Assert.Equal(StatusMapper.Done, StatusMapper.ToVietnamese(DeadlineStatus.Done));

            Assert.Equal(DeadlineStatus.Todo, StatusMapper.ToDbValue("Chưa làm"));
            Assert.Equal(DeadlineStatus.InProgress, StatusMapper.ToDbValue("Đang làm"));
            Assert.Equal(DeadlineStatus.Done, StatusMapper.ToDbValue("Hoàn thành"));
        }

        [Fact]
        public void TypeMapper_BidirectionalMapping_WorksCorrectly()
        {
            Assert.Equal(TypeMapper.Assignment, TypeMapper.ToVietnamese(DeadlineType.Assignment));
            Assert.Equal(TypeMapper.Exam, TypeMapper.ToVietnamese(DeadlineType.Exam));
            Assert.Equal(TypeMapper.Review, TypeMapper.ToVietnamese(DeadlineType.Review));

            Assert.Equal(DeadlineType.Assignment, TypeMapper.ToDbValue("Bài tập"));
            Assert.Equal(DeadlineType.Exam, TypeMapper.ToDbValue("Lịch thi"));
            Assert.Equal(DeadlineType.Review, TypeMapper.ToDbValue("Ôn tập"));
        }

        [Fact]
        public void CalculateCountdown_Overdue_ReturnsWarning()
        {
            var pastDate = DateTime.Now.AddHours(-2);
            var result = TaskViewModel.CalculateCountdown(pastDate);
            Assert.Contains("⚠ Đã quá hạn!", result);
        }

        [Fact]
        public void CalculateCountdown_FutureDate_ReturnsDaysAndHours()
        {
            var futureDate = DateTime.Now.AddDays(2).AddHours(3);
            var result = TaskViewModel.CalculateCountdown(futureDate);
            Assert.Contains("⏱ Còn 2 ngày", result);
        }

        [Fact]
        public void CalculateCountdown_Null_ReturnsNoDueDateText()
        {
            var result = TaskViewModel.CalculateCountdown(null);
            Assert.Equal("Chưa có hạn", result);
        }

        [Fact]
        public void TaskItem_PropertyChange_TriggersEvent()
        {
            var task = new TaskItem { Title = "Old Title" };
            bool propertyChangedFired = false;
            task.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TaskItem.Title))
                    propertyChangedFired = true;
            };

            task.Title = "New Title";
            Assert.True(propertyChangedFired);
            Assert.Equal("New Title", task.Title);
        }

        [Fact]
        public void DueDateToUrgencyColor_ReturnsRedForOverdue()
        {
            var converter = new DueDateToUrgencyColorConverter();
            var pastDate = DateTime.Now.AddHours(-1);

            var brush = converter.Convert(pastDate, typeof(Brush), string.Empty, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Color.FromRgb(239, 68, 68), brush.Color);
        }

        [Fact]
        public void DueDateToUrgencyColor_ReturnsOrangeForDueSoon()
        {
            var converter = new DueDateToUrgencyColorConverter();
            var soonDate = DateTime.Now.AddDays(2);

            var brush = converter.Convert(soonDate, typeof(Brush), string.Empty, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Color.FromRgb(249, 115, 22), brush.Color);
        }
    }
}
