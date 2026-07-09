using System.Windows;
using System.Windows.Controls;
using AIStudyHub.Views;

namespace AIStudyHub
{
    /// <summary>
    /// Code-behind cho MainWindow.xaml.
    /// Theo chuẩn MVVM, file này chỉ chứa UI Logic (Navigation).
    /// Không chứa bất kỳ logic nghiệp vụ nào.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Xử lý chuyển trang khi người dùng click vào nút navigation.
        /// Đây là UI Logic thuần túy (chuyển đổi giao diện), không phải nghiệp vụ.
        /// </summary>
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            // Reset màu nền tất cả nút nav về mặc định
            btnSubject.Background = System.Windows.Media.Brushes.Transparent;
            btnSubject.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(148, 163, 184));

            btnTask.Background = System.Windows.Media.Brushes.Transparent;
            btnTask.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(148, 163, 184));

            // Đánh dấu nút đang active
            btn.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(59, 130, 246));
            btn.Foreground = System.Windows.Media.Brushes.White;

            // Chuyển nội dung theo Tag của nút
            ContentArea.Child = btn.Tag?.ToString() switch
            {
                "Subject" => new SubjectView(),
                "Task" => new TaskView(),
                _ => new TaskView()
            };
        }
    }
}
