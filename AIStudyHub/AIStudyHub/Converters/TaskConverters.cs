using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AIStudyHub.Models;

namespace AIStudyHub.Converters
{
    /// <summary>
    /// Chuyển đổi chuỗi Status thành màu nền cho Kanban card.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                DeadlineStatus.Todo => new SolidColorBrush(Color.FromRgb(239, 246, 255)),       // xanh nhạt
                DeadlineStatus.InProgress => new SolidColorBrush(Color.FromRgb(255, 251, 235)), // vàng nhạt
                DeadlineStatus.Done => new SolidColorBrush(Color.FromRgb(240, 253, 244)),       // xanh lá nhạt
                _ => new SolidColorBrush(Colors.White)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi Status thành màu viền (border accent) cho Kanban card.
    /// </summary>
    public class StatusToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                DeadlineStatus.Todo => new SolidColorBrush(Color.FromRgb(96, 165, 250)),       // blue-400
                DeadlineStatus.InProgress => new SolidColorBrush(Color.FromRgb(251, 191, 36)), // amber-400
                DeadlineStatus.Done => new SolidColorBrush(Color.FromRgb(74, 222, 128)),       // green-400
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi loại Task thành biểu tượng emoji đơn giản.
    /// </summary>
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                DeadlineType.Assignment => "📝",
                DeadlineType.Exam => "📋",
                DeadlineType.Review => "🔍",
                _ => "📌"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi màu tag cho loại Task.
    /// </summary>
    public class TypeToTagColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                DeadlineType.Assignment => new SolidColorBrush(Color.FromRgb(165, 180, 252)),  // indigo-300
                DeadlineType.Exam => new SolidColorBrush(Color.FromRgb(252, 165, 165)),        // red-300
                DeadlineType.Review => new SolidColorBrush(Color.FromRgb(110, 231, 183)),      // emerald-300
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Tính màu cảnh báo dựa trên số ngày còn lại đến deadline.
    /// Đỏ = quá hạn hoặc còn &lt; 1 ngày, Cam = còn &lt; 3 ngày, Xanh = còn nhiều ngày.
    /// </summary>
    public class DueDateToUrgencyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dueDate)
                return new SolidColorBrush(Colors.Gray);

            var remaining = dueDate - DateTime.Now;
            if (remaining.TotalHours < 0)
                return new SolidColorBrush(Color.FromRgb(239, 68, 68));   // red-500
            if (remaining.TotalDays < 1)
                return new SolidColorBrush(Color.FromRgb(239, 68, 68));   // red-500
            if (remaining.TotalDays <= 3)
                return new SolidColorBrush(Color.FromRgb(249, 115, 22));  // orange-500
            return new SolidColorBrush(Color.FromRgb(34, 197, 94));       // green-500
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi DueDate thành chuỗi đếm ngược thân thiện.
    /// Logic được đặt inline để tránh phụ thuộc chéo giữa namespace Converters và ViewModels.
    /// </summary>
    public class DueDateToCountdownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dueDate)
                return "Chưa có hạn";

            var remaining = dueDate - DateTime.Now;
            if (remaining.TotalSeconds < 0)
                return "⚠ Đã quá hạn!";
            if (remaining.TotalDays >= 1)
                return $"⏱ Còn {(int)remaining.TotalDays} ngày {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            return $"⏱ Còn {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi nullable DateTime thành chuỗi hiển thị.
    /// </summary>
    public class NullableDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.ToString("dd/MM/yyyy HH:mm");
            return "Chưa đặt hạn";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Chuyển đổi bool thành Visibility. Mặc định true = Visible.
    /// Truyền parameter = "Invert" để đảo ngược.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            bool invert = parameter?.ToString() == "Invert";
            return (flag ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Kiểm tra task có đang quá hạn không (trả về true/false).
    /// </summary>
    public class IsOverdueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dueDate)
                return dueDate < DateTime.Now;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }
}
