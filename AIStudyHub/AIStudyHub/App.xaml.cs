using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace AIStudyHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cấu hình ngôn ngữ tiếng Việt (vi-VN) cho toàn bộ ứng dụng WPF (định dạng ngày dd/MM/yyyy)
            var culture = new CultureInfo("vi-VN");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            AIStudyHub.Data.AppDbContext.InitializeDatabase();
        }
    }
}
