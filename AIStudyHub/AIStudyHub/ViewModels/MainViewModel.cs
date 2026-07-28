using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Windows.Threading;
using AIStudyHub.Messages;

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
        private string _quickNoteText = string.Empty;

        [ObservableProperty]
        private bool _isChatLoading = false;

        [ObservableProperty]
        private bool _isNewChatWarningVisible = false;

        [ObservableProperty]
        private bool _doNotShowNewChatWarning = false;

        [ObservableProperty]
        private bool _isFocusTimerPopupOpen = false;

        [ObservableProperty]
        private bool _isTimerRunning = false;

        [ObservableProperty]
        private string _timerDisplay = "Hẹn giờ tập trung";

        [ObservableProperty]
        private int _customTimerHours = 0;

        [ObservableProperty]
        private int _customTimerMinutes = 25;

        private TimeSpan _timerRemaining;
        private DispatcherTimer? _focusTimer;

        public bool IsDashboardActive => CurrentViewModel is DashboardViewModel;
        public bool IsSubjectsActive => CurrentViewModel is SubjectViewModel;
        public bool IsTasksActive => CurrentViewModel is TaskViewModel;
        public bool IsDocumentsActive => CurrentViewModel is DocumentViewModel;
        public bool IsSettingsActive => CurrentViewModel is SettingsViewModel;
        public bool IsFlashcardsActive => CurrentViewModel is FlashcardViewModel || CurrentViewModel is FlashcardDeckViewModel;
        public bool IsNotesActive => CurrentViewModel is NoteViewModel;

        partial void OnCurrentViewModelChanged(ObservableObject? value)
        {
            OnPropertyChanged(nameof(IsDashboardActive));
            OnPropertyChanged(nameof(IsSubjectsActive));
            OnPropertyChanged(nameof(IsTasksActive));
            OnPropertyChanged(nameof(IsDocumentsActive));
            OnPropertyChanged(nameof(IsSettingsActive));
            OnPropertyChanged(nameof(IsFlashcardsActive));
            OnPropertyChanged(nameof(IsNotesActive));
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

            WeakReferenceMessenger.Default.Register<MainViewModel, ReviewDeckMessage>(this, (r, m) =>
            {
                r.CurrentViewModel = new FlashcardViewModel(m.Value);
            });

            WeakReferenceMessenger.Default.Register<MainViewModel, BackToDecksMessage>(this, (r, m) =>
            {
                r.CurrentViewModel = new FlashcardDeckViewModel();
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
        private void NavigateToFlashcards() => CurrentViewModel = new FlashcardDeckViewModel();

        [RelayCommand]
        private void NavigateToNotes() => CurrentViewModel = new NoteViewModel();

        [RelayCommand]
        private void NavigateToSettings() => CurrentViewModel = new SettingsViewModel();

        [RelayCommand]
        private void ToggleChat() => IsChatVisible = !IsChatVisible;

        [RelayCommand]
        private void ToggleFocusTimerPopup()
        {
            IsFocusTimerPopupOpen = !IsFocusTimerPopupOpen;
        }

        [RelayCommand]
        private void StartFocusTimer(string minutesStr)
        {
            if (int.TryParse(minutesStr, out int minutes))
            {
                StartTimer(TimeSpan.FromMinutes(minutes));
            }
        }

        [RelayCommand]
        private void StartCustomFocusTimer()
        {
            int hrs = CustomTimerHours;
            int mins = CustomTimerMinutes;
            
            // Fix negative inputs
            if (hrs < 0) hrs = 0;
            if (mins < 0) mins = 0;
            
            // Bound minutes
            if (mins > 59) mins = 59;
            
            // Bound max to 10h
            if (hrs > 10) 
            {
                hrs = 10;
                mins = 0;
            }
            if (hrs == 10 && mins > 0)
            {
                mins = 0;
            }
            
            // Bound min to 1m
            if (hrs == 0 && mins == 0)
            {
                mins = 1;
            }
            
            // Cập nhật lại UI
            CustomTimerHours = hrs;
            CustomTimerMinutes = mins;

            StartTimer(new TimeSpan(hrs, mins, 0));
        }

        [RelayCommand]
        private void StopFocusTimer()
        {
            if (_focusTimer != null)
            {
                _focusTimer.Stop();
                IsTimerRunning = false;
                TimerDisplay = "Hẹn giờ tập trung";
            }
        }

        private void StartTimer(TimeSpan duration)
        {
            if (duration.TotalSeconds <= 0) return;

            _timerRemaining = duration;
            IsTimerRunning = true;
            IsFocusTimerPopupOpen = false;
            UpdateTimerDisplay();

            if (_focusTimer == null)
            {
                _focusTimer = new DispatcherTimer();
                _focusTimer.Interval = TimeSpan.FromSeconds(1);
                _focusTimer.Tick += (s, e) =>
                {
                    if (_timerRemaining.TotalSeconds > 0)
                    {
                        _timerRemaining = _timerRemaining.Subtract(TimeSpan.FromSeconds(1));
                        UpdateTimerDisplay();
                    }
                    else
                    {
                        StopFocusTimer();
                        System.Media.SystemSounds.Exclamation.Play();
                        System.Windows.MessageBox.Show("Làm tốt lắm! Hãy nghỉ ngơi một chút trước khi tiếp tục nhé.", "Hết giờ!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                };
            }
            
            _focusTimer.Start();
        }

        private void UpdateTimerDisplay()
        {
            if (_timerRemaining.Hours > 0)
                TimerDisplay = $"{_timerRemaining.Hours:D2}:{_timerRemaining.Minutes:D2}:{_timerRemaining.Seconds:D2}";
            else
                TimerDisplay = $"{_timerRemaining.Minutes:D2}:{_timerRemaining.Seconds:D2}";
        }

        [RelayCommand]
        private void SaveQuickNote()
        {
            if (string.IsNullOrWhiteSpace(QuickNoteText)) return;

            using var db = new Data.AppDbContext();
            
            var quickNotes = System.Linq.Enumerable.FirstOrDefault(System.Linq.Queryable.Where(db.Notes, n => n.Title == "Ghi chú nhanh"));
            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            if (quickNotes == null)
            {
                quickNotes = new Models.Note
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Ghi chú nhanh",
                    Content = $"--- {timestamp} ---\n{QuickNoteText.Trim()}\n\n",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.Notes.Add(quickNotes);
            }
            else
            {
                quickNotes.Content += $"--- {timestamp} ---\n{QuickNoteText.Trim()}\n\n";
                quickNotes.UpdatedAt = DateTime.Now;
                db.Notes.Update(quickNotes);
            }

            db.SaveChanges();
            QuickNoteText = string.Empty;
            
            WeakReferenceMessenger.Default.Send(new NoteAddedMessage());
        }
    }
}
