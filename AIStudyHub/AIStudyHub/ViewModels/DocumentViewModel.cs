using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AIStudyHub.Data;
using AIStudyHub.Models;
using AIStudyHub.Services;

namespace AIStudyHub.ViewModels
{
    public partial class DocumentViewModel : ObservableObject
    {
        private readonly DocumentService _documentService;
        private readonly PdfViewerService _pdfViewerService;

        [ObservableProperty]
        private ObservableCollection<Document> _documents = new();

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        [ObservableProperty]
        private ObservableCollection<Subject> _filterSubjects = new();

        [ObservableProperty]
        private Subject? _selectedFilterSubject;

        [ObservableProperty]
        private Document? _selectedDocument;

        // Trạng thái PDF Viewer
        [ObservableProperty]
        private bool _isViewerOpen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWordDocument))]
        private bool _isPdfDocument = true;

        public bool IsWordDocument => !IsPdfDocument;

        [ObservableProperty]
        private string _documentTextContent = string.Empty;

        [ObservableProperty]
        private Uri? _currentViewerUrl;

        [ObservableProperty]
        private string _currentTitle = string.Empty;

        [ObservableProperty]
        private BitmapSource? _currentPageImage;

        [ObservableProperty]
        private int _currentPageNumber = 1;

        [ObservableProperty]
        private int _totalPages = 0;

        [ObservableProperty]
        private double _zoomFactor = 1.0;

        [ObservableProperty]
        private int _rotationDegrees = 0;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public string ZoomPercentageText => $"{(int)(ZoomFactor * 100)}%";

        public DocumentViewModel()
        {
            AppDbContext.InitializeDatabase();
            _documentService = new DocumentService();
            _pdfViewerService = new PdfViewerService();

            LoadSubjects();
            SelectedFilterSubject = FilterSubjects.FirstOrDefault();
            LoadDocuments();
        }

        public void LoadSubjects()
        {
            using var db = new AppDbContext();
            var list = db.Subjects.ToList();
            Subjects = new ObservableCollection<Subject>(list);

            var previousSelectedId = SelectedFilterSubject?.Id;

            var filterList = new List<Subject> { new Subject { Id = Guid.Empty, Name = "Tất cả môn học" } };
            filterList.AddRange(list);
            FilterSubjects = new ObservableCollection<Subject>(filterList);

            if (previousSelectedId.HasValue)
            {
                var reselected = FilterSubjects.FirstOrDefault(s => s.Id == previousSelectedId.Value);
                if (reselected != null)
                {
                    SelectedFilterSubject = reselected;
                }
                else
                {
                    SelectedFilterSubject = FilterSubjects.FirstOrDefault();
                }
            }
            else
            {
                SelectedFilterSubject = FilterSubjects.FirstOrDefault();
            }
        }

        public void LoadDocuments()
        {
            Guid? filterId = (SelectedFilterSubject == null || SelectedFilterSubject.Id == Guid.Empty)
                ? null
                : SelectedFilterSubject.Id;

            var list = _documentService.GetDocuments(filterId);
            Documents = new ObservableCollection<Document>(list);
            StatusMessage = $"Đã tải {Documents.Count} tài liệu.";
        }

        partial void OnSelectedFilterSubjectChanged(Subject? value)
        {
            LoadDocuments();
        }

        [RelayCommand]
        private async Task UploadDocumentAsync()
        {
            LoadSubjects();
            if (Subjects.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một Môn Học trước khi upload tài liệu!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Chọn các file tài liệu PDF hoặc Word để tải lên",
                Filter = "Tài liệu học tập (*.pdf;*.docx;*.doc)|*.pdf;*.docx;*.doc|PDF Files (*.pdf)|*.pdf|Word Files (*.docx;*.doc)|*.docx;*.doc|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true && dialog.FileNames.Length > 0)
            {
                var selectDialog = new Views.SubjectSelectionDialog(Subjects.ToList());
                if (selectDialog.ShowDialog() != true || selectDialog.SelectedSubject == null)
                {
                    StatusMessage = "Tải tài liệu bị hủy.";
                    return;
                }

                var targetSubject = selectDialog.SelectedSubject;
                int successCount = 0;
                Document? lastDoc = null;

                try
                {
                    StatusMessage = $"Đang tải lên {dialog.FileNames.Length} tài liệu...";
                    foreach (var fileName in dialog.FileNames)
                    {
                        try
                        {
                            var doc = await _documentService.UploadDocumentAsync(fileName, targetSubject.Id);
                            lastDoc = doc;
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi tải file {Path.GetFileName(fileName)}: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    LoadDocuments();
                    StatusMessage = $"Đã tải lên thành công {successCount} tài liệu.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "Tải file thất bại.";
                }
            }
        }

        [RelayCommand]
        private void OpenDocument(Document? document)
        {
            if (document == null) return;

            SelectedDocument = document;
            CurrentTitle = document.Title;
            IsViewerOpen = true;

            string displayPath = document.FilePath;

            if (string.Equals(document.FileType, "PDF", StringComparison.OrdinalIgnoreCase))
            {
                IsPdfDocument = true;
                StatusMessage = $"Đang đọc PDF: {document.Title} (Xem toàn bộ trang & Bôi đen copy chữ)";
            }
            else
            {
                IsPdfDocument = false;
                StatusMessage = $"Đang đọc Word: {document.Title} (Đúng bố cục Word không giới hạn trang & Bôi đen copy chữ)";

                try
                {
                    string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AIStudyHub", "Cache");
                    Directory.CreateDirectory(cacheDir);
                    displayPath = Path.Combine(cacheDir, $"{document.Id}.html");

                    if (!File.Exists(displayPath) || File.GetLastWriteTime(document.FilePath) > File.GetLastWriteTime(displayPath))
                    {
                        var converter = new Mammoth.DocumentConverter();
                        var result = converter.ConvertToHtml(document.FilePath);
                        string htmlBody = result.Value ?? string.Empty;

                        string styledHtml = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{
    font-family: 'Segoe UI', Calibri, Arial, sans-serif;
    font-size: 16px;
    line-height: 1.65;
    color: #1E293B;
    background-color: #F1F5F9;
    margin: 0;
    padding: 30px 20px;
  }}
  .word-page {{
    background: white;
    padding: 50px 65px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.08);
    border-radius: 8px;
    max-width: 860px;
    margin: 0 auto;
    min-height: 900px;
  }}
  table {{
    border-collapse: collapse;
    width: 100%;
    margin: 18px 0;
  }}
  table, th, td {{
    border: 1px solid #CBD5E1;
  }}
  th, td {{
    padding: 10px 14px;
    text-align: left;
  }}
  th {{
    background-color: #F8FAFC;
    font-weight: 600;
  }}
  h1, h2, h3, h4 {{
    color: #0F172A;
    margin-top: 1.4em;
    margin-bottom: 0.5em;
  }}
  p {{
    margin: 0.85em 0;
  }}
  img {{
    max-width: 100%;
    height: auto;
  }}
</style>
</head>
<body>
  <div class='word-page'>
    {htmlBody}
  </div>
</body>
</html>";
                        File.WriteAllText(displayPath, styledHtml, System.Text.Encoding.UTF8);
                    }
                }
                catch { }
            }

            try
            {
                CurrentViewerUrl = new Uri(displayPath);
            }
            catch { }
        }

        [RelayCommand]
        private void DeleteDocument(Document? document)
        {
            if (document == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài liệu \"{document.Title}\"?\nFile sẽ được xóa khỏi thư mục App Data.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                if (SelectedDocument?.Id == document.Id && IsViewerOpen)
                {
                    CloseViewer();
                }

                _documentService.DeleteDocument(document);
                LoadDocuments();
                StatusMessage = $"Đã xóa tài liệu: {document.Title}";
            }
        }

        [RelayCommand]
        private void CloseViewer()
        {
            _pdfViewerService.Dispose();
            CurrentPageImage = null;
            IsViewerOpen = false;
            StatusMessage = "Đã đóng trình đọc PDF.";
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPageNumber < TotalPages)
            {
                CurrentPageNumber++;
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPageNumber > 1)
            {
                CurrentPageNumber--;
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void FirstPage()
        {
            if (CurrentPageNumber != 1 && TotalPages > 0)
            {
                CurrentPageNumber = 1;
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void LastPage()
        {
            if (CurrentPageNumber != TotalPages && TotalPages > 0)
            {
                CurrentPageNumber = TotalPages;
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void ZoomIn()
        {
            if (ZoomFactor < 3.5)
            {
                ZoomFactor += 0.25;
                OnPropertyChanged(nameof(ZoomPercentageText));
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void ZoomOut()
        {
            if (ZoomFactor > 0.5)
            {
                ZoomFactor -= 0.25;
                OnPropertyChanged(nameof(ZoomPercentageText));
                RenderCurrentPage();
            }
        }

        [RelayCommand]
        private void ResetZoom()
        {
            ZoomFactor = 1.0;
            OnPropertyChanged(nameof(ZoomPercentageText));
            RenderCurrentPage();
        }

        [RelayCommand]
        private void RotateClockwise()
        {
            RotationDegrees = (RotationDegrees + 90) % 360;
            RenderCurrentPage();
        }

        public void RenderCurrentPage()
        {
            if (!_pdfViewerService.IsLoaded || TotalPages == 0)
            {
                CurrentPageImage = null;
                return;
            }

            // PdfiumViewer dùng 0-indexed cho trang
            CurrentPageImage = _pdfViewerService.RenderPage(CurrentPageNumber - 1, ZoomFactor, RotationDegrees);
        }
    }
}
