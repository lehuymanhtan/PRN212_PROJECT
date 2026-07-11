using System.Windows;
using System.Windows.Controls;

namespace AIStudyHub.Views
{
    public partial class DocumentView : UserControl
    {
        public DocumentView()
        {
            InitializeComponent();
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
