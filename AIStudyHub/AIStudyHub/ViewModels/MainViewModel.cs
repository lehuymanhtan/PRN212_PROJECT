using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIStudyHub.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _selectedTabIndex = 0;

        [RelayCommand]
        private void ShowSubjects()
        {
            SelectedTabIndex = 0;
        }

        [RelayCommand]
        private void ShowDocuments()
        {
            SelectedTabIndex = 1;
        }
    }
}
