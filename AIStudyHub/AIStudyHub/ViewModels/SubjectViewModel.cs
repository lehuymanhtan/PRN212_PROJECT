using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Models;
using AIStudyHub.Data;

namespace AIStudyHub.ViewModels
{
    // Bắt buộc kế thừa ObservableObject và dùng 'partial' class
    public partial class SubjectViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        // ObservableProperty tự động tạo INotifyPropertyChanged cho UI
        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        [ObservableProperty]
        private string _subjectName = string.Empty;

        [ObservableProperty]
        private string _courseCode = string.Empty;

        public SubjectViewModel()
        {
            _dbContext = new AppDbContext();

            // Lệnh này đảm bảo tự động tạo file .db nếu chưa có
            _dbContext.Database.EnsureCreated();

            LoadData();
        }

        private void LoadData()
        {
            // Tạo 1 User giả định nếu DB trống (vì Subject cần có UserId)
            var user = _dbContext.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Username = "TestAdmin", PasswordHash = "hashed_pwd" };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            // Tải danh sách môn học lên UI
            var list = _dbContext.Subjects.Where(s => s.UserId == user.Id).ToList();
            Subjects = new ObservableCollection<Subject>(list);
        }

        // Tự động tạo ICommand là AddSubjectCommand
        [RelayCommand]
        private void AddSubject()
        {
            if (string.IsNullOrWhiteSpace(SubjectName)) return;

            var user = _dbContext.Users.First();
            var newSubject = new Subject
            {
                Name = SubjectName,
                CourseCode = CourseCode,
                UserId = user.Id,
                ColorHex = "#CCCCCC" // Màu mặc định
            };

            // Lưu xuống DB
            _dbContext.Subjects.Add(newSubject);
            _dbContext.SaveChanges();

            // Cập nhật lại UI
            Subjects.Add(newSubject);

            // Reset form
            SubjectName = string.Empty;
            CourseCode = string.Empty;
        }

        [RelayCommand]
        private void DeleteSubject(Subject? subject)
        {
            if (subject == null) return;

            // Xoá dưới DB
            _dbContext.Subjects.Remove(subject);
            _dbContext.SaveChanges();

            // Xoá trên UI
            Subjects.Remove(subject);
        }
    }
}
