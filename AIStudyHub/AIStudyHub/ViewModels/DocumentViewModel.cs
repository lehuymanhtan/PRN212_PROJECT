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
        private Subject? _selectedFilterSubject;

        [ObservableProperty]
        private Document? _selectedDocument;

        // Trạng thái PDF Viewer
        [ObservableProperty]
        private bool _isViewerOpen;

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
            LoadDocuments();
        }

        public void LoadSubjects()
        {
            using var db = new AppDbContext();
            var list = db.Subjects.ToList();
            Subjects = new ObservableCollection<Subject>(list);
        }

        public void LoadDocuments()
        {
            var list = _documentService.GetDocuments(SelectedFilterSubject?.Id);
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
                Title = "Chọn file tài liệu PDF hoặc Word để tải lên",
                Filter = "Tài liệu học tập (*.pdf;*.docx;*.doc)|*.pdf;*.docx;*.doc|PDF Files (*.pdf)|*.pdf|Word Files (*.docx;*.doc)|*.docx;*.doc|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                var targetSubject = SelectedFilterSubject ?? Subjects.First();

                try
                {
                    StatusMessage = "Đang tải tài liệu vào App Data...";
                    var doc = await _documentService.UploadDocumentAsync(dialog.FileName, targetSubject.Id);
                    LoadDocuments();
                    StatusMessage = $"Đã tải lên thành công: {doc.Title}";

                    // Tự động mở ngay nếu là PDF
                    if (doc.FileType == "PDF")
                    {
                        OpenDocument(doc);
                    }
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

            if (document.FileType == "PDF")
            {
                try
                {
                    _pdfViewerService.LoadPdf(document.FilePath);
                    TotalPages = _pdfViewerService.PageCount;
                    CurrentPageNumber = 1;
                    ZoomFactor = 1.0;
                    RotationDegrees = 0;
                    CurrentTitle = document.Title;
                    IsViewerOpen = true;

                    RenderCurrentPage();
                    StatusMessage = $"Đang đọc PDF: {document.Title} ({TotalPages} trang)";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể mở file PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Nếu là Word DOCX, hỏi người dùng có muốn mở bằng Word của hệ thống không
                var result = MessageBox.Show(
                    $"File \"{document.Title}\" là định dạng Word ({document.FileType}).\nBạn có muốn mở file này bằng ứng dụng mặc định của Windows không?",
                    "Mở tài liệu Word", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(document.FilePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi mở file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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
