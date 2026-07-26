using System.Windows.Controls;
using AIStudyHub.ViewModels;

namespace AIStudyHub.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
