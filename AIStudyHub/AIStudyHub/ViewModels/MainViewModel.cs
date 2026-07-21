using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIStudyHub.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? _currentViewModel;

        [ObservableProperty]
        private bool _isChatVisible = false;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<Models.ChatMessage> _chatMessages = new();

        [ObservableProperty]
        private string _chatInputText = string.Empty;

        [ObservableProperty]
        private bool _isChatLoading = false;

        [ObservableProperty]
        private bool _isNewChatWarningVisible = false;

        [ObservableProperty]
        private bool _doNotShowNewChatWarning = false;

        public bool IsDashboardActive => CurrentViewModel is DashboardViewModel;
        public bool IsSubjectsActive => CurrentViewModel is SubjectViewModel;
        public bool IsTasksActive => CurrentViewModel is TaskViewModel;
        public bool IsDocumentsActive => CurrentViewModel is DocumentViewModel;
        public bool IsSettingsActive => CurrentViewModel is SettingsViewModel;

        partial void OnCurrentViewModelChanged(ObservableObject? value)
        {
            OnPropertyChanged(nameof(IsDashboardActive));
            OnPropertyChanged(nameof(IsSubjectsActive));
            OnPropertyChanged(nameof(IsTasksActive));
            OnPropertyChanged(nameof(IsDocumentsActive));
            OnPropertyChanged(nameof(IsSettingsActive));
        }


        public MainViewModel()
        {
            _currentViewModel = new TaskViewModel();
            ClearAllChatHistoryOnStartup();

            WeakReferenceMessenger.Default.Register<MainViewModel, ValueChangedMessage<string>>(this, (r, m) => 
            {
                if (m.Value.StartsWith("ContextualAI|"))
                {
                    r.IsChatVisible = true;
                    r.ChatInputText = m.Value.Substring(13);
                    _ = r.SendChatMessageAsync();
                }
            });
        }

        private void ClearAllChatHistoryOnStartup()
        {
            using var db = new Data.AppDbContext();
            db.ChatMessages.RemoveRange(db.ChatMessages);
            db.SaveChanges();
            ChatMessages.Clear();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task SendChatMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(ChatInputText) || IsChatLoading) return;

            // Xác định xem user có đang mở Document nào không
            System.Guid? currentDocId = null;
            if (CurrentViewModel is DocumentViewModel docVm && docVm.SelectedDocument != null)
            {
                currentDocId = docVm.SelectedDocument.Id;
            }

            var userMsg = new Models.ChatMessage 
            { 
                DocumentId = currentDocId, // Sử dụng null cho Global, hoặc ID của tài liệu nếu đang mở
                Role = "user", 
                Content = ChatInputText 
            };
            
            ChatMessages.Add(userMsg);
            var question = ChatInputText;
            ChatInputText = string.Empty;
            IsChatLoading = true;

            using (var db = new Data.AppDbContext())
            {
                db.ChatMessages.Add(userMsg);
                await db.SaveChangesAsync();
            }

            var aiService = new Services.AIService();
            // Truyền currentDocId để AIService biết nếu đang đọc doc thì dùng chunk của doc đó
            var response = await aiService.AskQuestionAsync(currentDocId ?? System.Guid.Empty, question, System.Linq.Enumerable.ToList(ChatMessages));

            var aiMsg = new Models.ChatMessage 
            { 
                DocumentId = currentDocId, 
                Role = "model", 
                Content = response 
            };
            
            ChatMessages.Add(aiMsg);
            
            using (var db = new Data.AppDbContext())
            {
                db.ChatMessages.Add(aiMsg);
                await db.SaveChangesAsync();
            }

            IsChatLoading = false;
        }

        [RelayCommand]
        private void StartNewChat()
        {
            if (ChatMessages.Count == 0) return;

            using var db = new Data.AppDbContext();
            var setting = db.AppSettings.Find("SkipNewChatWarning");
            if (setting?.Value == "True")
            {
                ClearChat();
                return;
            }

            IsNewChatWarningVisible = true;
        }

        [RelayCommand]
        private void ConfirmNewChat()
        {
            if (DoNotShowNewChatWarning)
            {
                using var db = new Data.AppDbContext();
                var setting = db.AppSettings.Find("SkipNewChatWarning");
                if (setting == null)
                {
                    db.AppSettings.Add(new Models.AppSetting { Key = "SkipNewChatWarning", Value = "True" });
                }
                else
                {
                    setting.Value = "True";
                }
                db.SaveChanges();
            }

            IsNewChatWarningVisible = false;
            ClearChat();
        }

        [RelayCommand]
        private void CancelNewChat()
        {
            IsNewChatWarningVisible = false;
        }

        private void ClearChat()
        {
            System.Guid? currentDocId = null;
            if (CurrentViewModel is DocumentViewModel docVm && docVm.SelectedDocument != null)
            {
                currentDocId = docVm.SelectedDocument.Id;
            }

            using var db = new Data.AppDbContext();
            var messagesToDelete = System.Linq.Enumerable.ToList(System.Linq.Queryable.Where(db.ChatMessages, c => c.DocumentId == currentDocId));
            db.ChatMessages.RemoveRange(messagesToDelete);
            db.SaveChanges();

            ChatMessages.Clear();
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
        private void NavigateToSettings() => CurrentViewModel = new SettingsViewModel();

        [RelayCommand]
        private void ToggleChat() => IsChatVisible = !IsChatVisible;
    }
}
