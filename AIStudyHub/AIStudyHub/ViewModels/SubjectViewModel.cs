using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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
            AppDbContext.InitializeDatabase();
            _dbContext = new AppDbContext();

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

            var result = MessageBox.Show(
                "Nếu môn học bị xóa, các tài liệu của môn học cũng sẽ bị xóa. Bạn có chắc chắn muốn xóa môn học này không?",
                "Xác nhận xóa môn học",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Tìm tất cả tài liệu của môn học này để xoá file vật lý & xoá dưới DB
                var documents = _dbContext.Documents.Where(d => d.SubjectId == subject.Id).ToList();
                foreach (var doc in documents)
                {
                    if (!string.IsNullOrEmpty(doc.FilePath) && File.Exists(doc.FilePath))
                    {
                        try
                        {
                            File.Delete(doc.FilePath);
                        }
                        catch { }
                    }
                    _dbContext.Documents.Remove(doc);
                }

                _dbContext.Subjects.Remove(subject);
                _dbContext.SaveChanges();

                Subjects.Remove(subject);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa môn học: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
