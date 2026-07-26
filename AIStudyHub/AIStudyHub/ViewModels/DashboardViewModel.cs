using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _recentTasks = new();

        [ObservableProperty]
        private int _totalSubjects;

        [ObservableProperty]
        private int _totalTasks;

        [ObservableProperty]
        private int _pendingTasks;

        [ObservableProperty]
        private int _overdueTasks;

        public DashboardViewModel()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            _dbContext.EnsureTaskTableCreated();

            LoadData();
        }

        public void LoadData()
        {
            var user = _dbContext.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Username = "TestAdmin", PasswordHash = "hashed_pwd" };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            var subjectList = _dbContext.Subjects
                .Where(s => s.UserId == user.Id)
                .ToList();

            Subjects = new ObservableCollection<Subject>(subjectList);

            var taskList = _dbContext.Tasks
                .Include(t => t.Subject)
                .OrderBy(t => t.DueDate)
                .ToList();

            RecentTasks = new ObservableCollection<TaskItem>(taskList);

            var now = DateTime.Now;
            TotalSubjects = subjectList.Count;
            TotalTasks = taskList.Count;
            PendingTasks = taskList.Count(t => StatusMapper.ToDbValue(t.Status) != DeadlineStatus.Done);
            OverdueTasks = taskList.Count(t => t.DueDate.HasValue && t.DueDate.Value < now && StatusMapper.ToDbValue(t.Status) != DeadlineStatus.Done);
        }

        [RelayCommand]
        private void RefreshData()
        {
            LoadData();
        }

        [RelayCommand]
        private void MarkTaskDone(TaskItem? task)
        {
            if (task == null) return;

            task.Status = DeadlineStatus.Done;
            _dbContext.Tasks.Update(task);
            _dbContext.SaveChanges();

            LoadData();
        }
    }
}
