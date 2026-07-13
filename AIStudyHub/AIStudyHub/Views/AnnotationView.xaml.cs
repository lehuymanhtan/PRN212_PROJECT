using System.Windows.Controls;
using System.Windows.Input;
using AIStudyHub.ViewModels;

namespace AIStudyHub.Views
{
    public partial class AnnotationView : UserControl
    {
        public AnnotationView()
        {
            InitializeComponent();
            Loaded += AnnotationView_Loaded;
        }

        private void AnnotationView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AnnotationViewModel vm)
            {
                vm.LoadAnnotationsForDocument("sample_doc_id"); // Dummy doc for demo
            }
        }

        private void PdfCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as AnnotationViewModel;
            if (vm == null) return;

            var pos = e.GetPosition(PdfCanvas);
            string content = AnnotationContentText.Text;

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "Empty note";
            }

            if (HighlightRadio.IsChecked == true)
            {
                vm.AddHighlight(1, pos.X, pos.Y, content);
            }
            else if (NoteRadio.IsChecked == true)
            {
                vm.AddNote(1, pos.X, pos.Y, content);
            }
        }
    }
}
