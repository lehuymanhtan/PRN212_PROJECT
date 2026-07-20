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
                
                DocumentWebView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
                DocumentWebView.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;

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

        private void CoreWebView2_ContextMenuRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs e)
        {
            if (e.ContextMenuTarget != null && e.ContextMenuTarget.HasSelection && !string.IsNullOrWhiteSpace(e.ContextMenuTarget.SelectionText))
            {
                var selection = e.ContextMenuTarget.SelectionText;
                var items = e.MenuItems;

                var translateItem = DocumentWebView.CoreWebView2.Environment.CreateContextMenuItem("Dịch sang tiếng Việt (AI)", null, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuItemKind.Command);
                translateItem.CustomItemSelected += (s, args) => TriggerContextualAI("Translate", selection);
                
                var summarizeItem = DocumentWebView.CoreWebView2.Environment.CreateContextMenuItem("Tóm tắt đoạn này (AI)", null, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuItemKind.Command);
                summarizeItem.CustomItemSelected += (s, args) => TriggerContextualAI("Summarize", selection);
                
                var explainItem = DocumentWebView.CoreWebView2.Environment.CreateContextMenuItem("Giải thích thuật ngữ (AI)", null, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuItemKind.Command);
                explainItem.CustomItemSelected += (s, args) => TriggerContextualAI("Explain", selection);

                items.Insert(0, explainItem);
                items.Insert(0, summarizeItem);
                items.Insert(0, translateItem);
            }
        }

        private void CoreWebView2_ContainsFullScreenElementChanged(object? sender, object e)
        {
            if (DocumentWebView.CoreWebView2.ContainsFullScreenElement)
            {
                // Force exit full screen because WPF doesn't handle WebView2 full screen natively
                // and it causes the PDF viewer toolbar to hide/become inaccessible.
                DocumentWebView.CoreWebView2.ExecuteScriptAsync("document.exitFullscreen();");
            }
        }

        private void TriggerContextualAI(string action, string text)
        {
            if (DataContext is ViewModels.DocumentViewModel vm)
            {
                vm.ContextualAICommand.Execute($"{action}|{text}");
            }
        }
    }
}
