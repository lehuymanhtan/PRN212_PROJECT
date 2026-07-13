using System.Windows;
using System.Windows.Controls;

namespace AIStudyHub.Views
{
    public partial class DocumentView : UserControl
    {
        public DocumentView()
        {
            InitializeComponent();
            this.Loaded += DocumentView_Loaded;
        }

        private async void DocumentView_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize WebView2 to ensure PDF reader works reliably
            try
            {
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "AIStudyHub", "WebView2"));
                await DocumentWebView.EnsureCoreWebView2Async(env);
                
                if (DataContext is ViewModels.DocumentViewModel vm && vm.CurrentViewerUrl != null)
                {
                    DocumentWebView.CoreWebView2.Navigate(vm.CurrentViewerUrl.ToString());
                }
            }
            catch
            {
                // Ignore initialization errors
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool isVisible && isVisible)
            {
                if (DataContext is ViewModels.DocumentViewModel vm)
                {
                    vm.LoadSubjects();
                    vm.LoadDocuments();
                }
            }
        }
    }
}
