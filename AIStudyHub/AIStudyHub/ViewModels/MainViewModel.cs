using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIStudyHub.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentViewModel;

        [ObservableProperty]
        private bool _isChatVisible = false;

        public MainViewModel()
        {
            CurrentViewModel = new DashboardViewModel();
        }

        [RelayCommand]
        private void NavigateToDashboard() => CurrentViewModel = new DashboardViewModel();

        [RelayCommand]
        private void NavigateToSubjects() => CurrentViewModel = new SubjectViewModel();

        [RelayCommand]
        private void ToggleChat() => IsChatVisible = !IsChatVisible;
    }
}
