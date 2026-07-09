using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.ViewModels
{
    /// <summary>
    /// ViewModel chính cho màn hình Quản lý Task/Deadline.
    /// Xử lý toàn bộ logic: CRUD, Kanban, Countdown Timer, Filter.
    /// </summary>
    public partial class TaskViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        // Timer đếm ngược - cập nhật mỗi 1 giây
        private readonly DispatcherTimer _countdownTimer;

        // =====================================================================
        // PROPERTIES - Danh sách dữ liệu
        // =====================================================================

        [ObservableProperty]
        private ObservableCollection<TaskItem> _allTasks = new();

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        // View dùng cho Kanban (nhóm theo Status)
        private ICollectionView? _tasksView;
        public ICollectionView? TasksView
        {
            get => _tasksView;
            private set => SetProperty(ref _tasksView, value);
        }

        // =====================================================================
        // PROPERTIES - Trạng thái UI / Form
        // =====================================================================

        [ObservableProperty]
        private string _currentDateTime = string.Empty;

        [ObservableProperty]
        private TaskItem? _selectedTask;

        [ObservableProperty]
        private bool _isFormOpen = false;

        [ObservableProperty]
        private bool _isEditMode = false;

        // =====================================================================
        // PROPERTIES - Dữ liệu Form nhập liệu
        // =====================================================================

        [ObservableProperty]
        private string _formTitle = string.Empty;

        [ObservableProperty]
        private string? _formDescription;

        [ObservableProperty]
        private DateTime _formDueDate = DateTime.Now.AddDays(7);

        [ObservableProperty]
        private string _formStatus = DeadlineStatus.Todo;

        [ObservableProperty]
        private string _formType = DeadlineType.Assignment;

        [ObservableProperty]
        private Subject? _formSubject;

        // =====================================================================
        // PROPERTIES - Bộ lọc (Filter)
        // =====================================================================

        [ObservableProperty]
        private string _filterStatus = "All";

        [ObservableProperty]
        private string _filterType = "All";

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private bool _showUrgentOnly = false;

        // =====================================================================
        // PROPERTIES - Thống kê tổng quan (Dashboard Counters)
        // =====================================================================

        [ObservableProperty]
        private int _countTodo;

        [ObservableProperty]
        private int _countInProgress;

        [ObservableProperty]
        private int _countDone;

        [ObservableProperty]
        private int _countOverdue;

        [ObservableProperty]
        private int _countDueSoon; // Deadline trong vòng 3 ngày

        // =====================================================================
        // COLLECTIONS - Các lựa chọn cho ComboBox
        // =====================================================================

        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            DeadlineStatus.Todo, DeadlineStatus.InProgress, DeadlineStatus.Done
        };

        public ObservableCollection<string> TypeOptions { get; } = new()
        {
            DeadlineType.Assignment, DeadlineType.Exam, DeadlineType.Review
        };

        public ObservableCollection<string> FilterStatusOptions { get; } = new()
        {
            "All", DeadlineStatus.Todo, DeadlineStatus.InProgress, DeadlineStatus.Done
        };

        public ObservableCollection<string> FilterTypeOptions { get; } = new()
        {
            "All", DeadlineType.Assignment, DeadlineType.Exam, DeadlineType.Review
        };

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================

        public TaskViewModel()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();

            // Đảm bảo bảng TASK tồn tại ngay cả khi DB được tạo trước khi thêm module TV3.
            // CREATE TABLE IF NOT EXISTS nên an toàn tuyệt đối, không xóa dữ liệu cũ.
            _dbContext.EnsureTaskTableCreated();

            // Khởi tạo countdown timer - tick mỗi 1 giây để cập nhật countdown
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += OnCountdownTimerTick;
            _countdownTimer.Start();

            LoadData();
        }

        // =====================================================================
        // PRIVATE METHODS - Load dữ liệu
        // =====================================================================

        private void LoadData()
        {
            // Đảm bảo có User mặc định (phối hợp với TV1 - SubjectViewModel)
            var user = _dbContext.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Username = "TestAdmin", PasswordHash = "hashed_pwd" };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            // Tải danh sách môn học vào ComboBox
            var subjectList = _dbContext.Subjects.Where(s => s.UserId == user.Id).ToList();
            Subjects = new ObservableCollection<Subject>(subjectList);

            // Tải tất cả tasks (include Subject để hiện tên môn học)
            var taskList = _dbContext.Tasks
                .OrderBy(t => t.DueDate)
                .ToList();
            AllTasks = new ObservableCollection<TaskItem>(taskList);

            // Khởi tạo CollectionView để hỗ trợ grouping và filtering
            SetupCollectionView();
            UpdateStatistics();
        }

        /// <summary>
        /// Thiết lập ICollectionView để nhóm theo Status (Kanban) và lọc theo điều kiện.
        /// Dùng ICollectionView thay vì tạo nhiều ObservableCollection riêng lẻ để tối ưu hiệu suất.
        /// </summary>
        private void SetupCollectionView()
        {
            TasksView = CollectionViewSource.GetDefaultView(AllTasks);

            // Nhóm theo Status để render Kanban board
            TasksView.GroupDescriptions.Clear();
            TasksView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TaskItem.Status)));

            // Sắp xếp theo DueDate tăng dần (deadline gần nhất lên đầu)
            TasksView.SortDescriptions.Clear();
            TasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.DueDate), ListSortDirection.Ascending));

            // Áp dụng bộ lọc
            TasksView.Filter = ApplyFilter;
        }

        /// <summary>
        /// Hàm lọc kết hợp: theo Status, Type, từ khoá tìm kiếm, và chế độ "Gấp" (deadline sắp đến).
        /// </summary>
        private bool ApplyFilter(object item)
        {
            if (item is not TaskItem task) return false;

            // Lọc theo Status
            if (FilterStatus != "All" && task.Status != FilterStatus)
                return false;

            // Lọc theo Type
            if (FilterType != "All" && task.Type != FilterType)
                return false;

            // Lọc theo từ khoá tìm kiếm (tìm trong Title và Description)
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.Trim().ToLower();
                var titleMatch = task.Title.ToLower().Contains(keyword);
                var descMatch = task.Description?.ToLower().Contains(keyword) ?? false;
                if (!titleMatch && !descMatch) return false;
            }

            // Lọc chế độ "Chỉ hiện Deadline gấp" - trong vòng 3 ngày
            if (ShowUrgentOnly)
            {
                if (!task.DueDate.HasValue) return false;
                var daysLeft = (task.DueDate.Value - DateTime.Now).TotalDays;
                // Bỏ qua task đã hoàn thành và task không gấp
                if (task.Status == DeadlineStatus.Done || daysLeft < 0 || daysLeft > 3) return false;
            }

            return true;
        }

        // =====================================================================
        // PRIVATE METHODS - Cập nhật thống kê
        // =====================================================================

        private void UpdateStatistics()
        {
            var now = DateTime.Now;
            CountTodo = AllTasks.Count(t => t.Status == DeadlineStatus.Todo);
            CountInProgress = AllTasks.Count(t => t.Status == DeadlineStatus.InProgress);
            CountDone = AllTasks.Count(t => t.Status == DeadlineStatus.Done);

            // Đếm task đã quá hạn (DueDate < now và chưa Done)
            CountOverdue = AllTasks.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < now &&
                t.Status != DeadlineStatus.Done);

            // Đếm task sắp đến hạn trong 3 ngày (nhưng chưa quá hạn và chưa Done)
            CountDueSoon = AllTasks.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value >= now &&
                (t.DueDate.Value - now).TotalDays <= 3 &&
                t.Status != DeadlineStatus.Done);
        }

        // =====================================================================
        // PRIVATE METHODS - Countdown Timer Tick
        // =====================================================================

        /// <summary>
        /// Gọi mỗi giây để cập nhật hiển thị đồng hồ thời gian thực.
        /// Thay vì notify từng TaskItem, chỉ cập nhật property CurrentDateTime trên ViewModel.
        /// </summary>
        private void OnCountdownTimerTick(object? sender, EventArgs e)
        {
            CurrentDateTime = DateTime.Now.ToString("HH:mm:ss - dddd, dd/MM/yyyy");

            // Cập nhật lại thống kê mỗi phút để phản ánh task vừa quá hạn
            if (DateTime.Now.Second == 0)
            {
                UpdateStatistics();
                // Refresh filter để task quá hạn được highlight lại
                TasksView?.Refresh();
            }
        }

        // =====================================================================
        // COMPUTED PROPERTY - Đếm ngược cho task đang chọn
        // =====================================================================

        /// <summary>
        /// Tính chuỗi đếm ngược cho task được chọn.
        /// Cập nhật mỗi giây thông qua việc gọi OnPropertyChanged từ timer.
        /// </summary>
        public string SelectedTaskCountdown
        {
            get => CalculateCountdown(SelectedTask?.DueDate);
        }

        public static string CalculateCountdown(DateTime? dueDate)
        {
            if (!dueDate.HasValue) return "Chưa có hạn";
            var remaining = dueDate.Value - DateTime.Now;
            if (remaining.TotalSeconds < 0)
                return "⚠ Đã quá hạn!";
            if (remaining.TotalDays >= 1)
                return $"⏱ Còn {(int)remaining.TotalDays} ngày {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            return $"⏱ Còn {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
        }

        // Gọi OnPropertyChanged cho SelectedTaskCountdown mỗi khi CurrentDateTime thay đổi
        partial void OnCurrentDateTimeChanged(string value)
        {
            OnPropertyChanged(nameof(SelectedTaskCountdown));
        }

        // Cập nhật countdown khi chọn task khác
        partial void OnSelectedTaskChanged(TaskItem? value)
        {
            OnPropertyChanged(nameof(SelectedTaskCountdown));
        }

        // =====================================================================
        // PARTIAL METHODS - Tự động refresh khi filter thay đổi
        // =====================================================================

        partial void OnFilterStatusChanged(string value) => TasksView?.Refresh();
        partial void OnFilterTypeChanged(string value) => TasksView?.Refresh();
        partial void OnSearchKeywordChanged(string value) => TasksView?.Refresh();
        partial void OnShowUrgentOnlyChanged(bool value) => TasksView?.Refresh();

        // =====================================================================
        // RELAY COMMANDS - Mở/Đóng Form
        // =====================================================================

        [RelayCommand]
        private void OpenAddForm()
        {
            IsEditMode = false;
            IsFormOpen = true;

            // Reset form về trạng thái trống
            FormTitle = string.Empty;
            FormDescription = null;
            FormDueDate = DateTime.Now.AddDays(7);
            FormStatus = DeadlineStatus.Todo;
            FormType = DeadlineType.Assignment;
            FormSubject = Subjects.FirstOrDefault();
        }

        [RelayCommand]
        private void OpenEditForm(TaskItem? task)
        {
            if (task == null) return;

            IsEditMode = true;
            IsFormOpen = true;
            SelectedTask = task;

            // Điền dữ liệu hiện tại vào form
            FormTitle = task.Title;
            FormDescription = task.Description;
            FormDueDate = task.DueDate ?? DateTime.Now.AddDays(7);
            FormStatus = task.Status;
            FormType = task.Type;
            FormSubject = Subjects.FirstOrDefault(s => s.Id == task.SubjectId);
        }

        [RelayCommand]
        private void CloseForm()
        {
            IsFormOpen = false;
        }

        // =====================================================================
        // RELAY COMMANDS - CRUD Operations
        // =====================================================================

        [RelayCommand]
        private void SaveTask()
        {
            if (string.IsNullOrWhiteSpace(FormTitle)) return;
            if (FormSubject == null && Subjects.Any())
            {
                // Nếu chưa chọn môn học, tự chọn môn đầu tiên
                FormSubject = Subjects.First();
            }

            if (IsEditMode)
            {
                UpdateTask();
            }
            else
            {
                AddTask();
            }

            IsFormOpen = false;
        }

        private void AddTask()
        {
            // Nếu không có môn học nào thì tạo task không có môn học
            var subjectId = FormSubject?.Id ?? Guid.Empty;

            // Chỉ thêm task nếu có SubjectId hợp lệ
            if (subjectId == Guid.Empty)
            {
                // Kiểm tra xem có Subject nào không
                var firstSubject = _dbContext.Subjects.FirstOrDefault();
                if (firstSubject == null) return; // Cần có ít nhất 1 môn học
                subjectId = firstSubject.Id;
                FormSubject = firstSubject;
            }

            var newTask = new TaskItem
            {
                Title = FormTitle.Trim(),
                Description = FormDescription?.Trim(),
                DueDate = FormDueDate,
                Status = FormStatus,
                Type = FormType,
                SubjectId = subjectId
            };

            _dbContext.Tasks.Add(newTask);
            _dbContext.SaveChanges();

            // Gán Subject để UI hiển thị tên môn học ngay mà không cần reload
            newTask.Subject = FormSubject;
            AllTasks.Add(newTask);

            UpdateStatistics();
        }

        private void UpdateTask()
        {
            if (SelectedTask == null) return;

            // Cập nhật trực tiếp trên object đang track bởi EF Core
            SelectedTask.Title = FormTitle.Trim();
            SelectedTask.Description = FormDescription?.Trim();
            SelectedTask.DueDate = FormDueDate;
            SelectedTask.Status = FormStatus;
            SelectedTask.Type = FormType;

            if (FormSubject != null)
            {
                SelectedTask.SubjectId = FormSubject.Id;
                SelectedTask.Subject = FormSubject;
            }

            _dbContext.Tasks.Update(SelectedTask);
            _dbContext.SaveChanges();

            // Refresh CollectionView để áp lại sort/filter sau khi sửa
            TasksView?.Refresh();
            UpdateStatistics();
        }

        [RelayCommand]
        private void DeleteTask(TaskItem? task)
        {
            if (task == null) return;

            _dbContext.Tasks.Remove(task);
            _dbContext.SaveChanges();

            AllTasks.Remove(task);
            UpdateStatistics();
        }

        /// <summary>
        /// Chuyển trạng thái Task nhanh từ Kanban board (kéo thả ảo qua button).
        /// </summary>
        [RelayCommand]
        private void MoveTaskStatus(TaskItem? task)
        {
            if (task == null) return;

            // Xoay vòng trạng thái: Todo -> InProgress -> Done -> Todo
            task.Status = task.Status switch
            {
                DeadlineStatus.Todo => DeadlineStatus.InProgress,
                DeadlineStatus.InProgress => DeadlineStatus.Done,
                DeadlineStatus.Done => DeadlineStatus.Todo,
                _ => DeadlineStatus.Todo
            };

            _dbContext.Tasks.Update(task);
            _dbContext.SaveChanges();

            // Thông báo UI cập nhật lại grouping
            TasksView?.Refresh();
            UpdateStatistics();
        }

        // =====================================================================
        // RELAY COMMANDS - Filter shortcuts
        // =====================================================================

        [RelayCommand]
        private void ClearFilters()
        {
            FilterStatus = "All";
            FilterType = "All";
            SearchKeyword = string.Empty;
            ShowUrgentOnly = false;
        }

        [RelayCommand]
        private void ShowUrgentFilter()
        {
            ShowUrgentOnly = !ShowUrgentOnly;
        }
    }
}
