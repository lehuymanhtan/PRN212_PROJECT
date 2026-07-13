using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIStudyHub.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? _currentViewModel;

        [ObservableProperty]
        private bool _isChatVisible = false;

        public bool IsDashboardActive => CurrentViewModel is DashboardViewModel;
        public bool IsSubjectsActive => CurrentViewModel is SubjectViewModel;
        public bool IsTasksActive => CurrentViewModel is TaskViewModel;
        public bool IsDocumentsActive => CurrentViewModel is DocumentViewModel;

        partial void OnCurrentViewModelChanged(ObservableObject? value)
        {
            OnPropertyChanged(nameof(IsDashboardActive));
            OnPropertyChanged(nameof(IsSubjectsActive));
            OnPropertyChanged(nameof(IsTasksActive));
            OnPropertyChanged(nameof(IsDocumentsActive));
        }


        public MainViewModel()
        {
            _currentViewModel = new TaskViewModel();
        }

        [RelayCommand]
        private void NavigateToDashboard() => CurrentViewModel = new DashboardViewModel();

        [RelayCommand]
        private void NavigateToSubjects() => CurrentViewModel = new SubjectViewModel();

        [RelayCommand]
        private void NavigateToTasks() => CurrentViewModel = new TaskViewModel();

        [RelayCommand]
        private void NavigateToDocuments() => CurrentViewModel = new DocumentViewModel();

        [RelayCommand]
        private void ToggleChat() => IsChatVisible = !IsChatVisible;
    }
}
