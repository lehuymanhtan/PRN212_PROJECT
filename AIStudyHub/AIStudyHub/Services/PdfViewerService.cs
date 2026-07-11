using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using PdfiumViewer;

namespace AIStudyHub.Services
{
    public class PdfViewerService : IDisposable
    {
        private PdfDocument? _pdfDocument;
        private string? _currentFilePath;

        public bool IsLoaded => _pdfDocument != null;
        public int PageCount => _pdfDocument?.PageCount ?? 0;
        public string? CurrentFilePath => _currentFilePath;

        /// <summary>
        /// Tải file PDF từ đường dẫn cục bộ
        /// </summary>
        public void LoadPdf(string filePath)
        {
            Dispose();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Không tìm thấy file PDF.", filePath);
            }

            _pdfDocument = PdfDocument.Load(filePath);
            _currentFilePath = filePath;
        }

        /// <summary>
        /// Render trang PDF thành BitmapSource WPF với Zoom và Góc quay (Rotate)
        /// </summary>
        /// <param name="pageIndex">Index trang (0-based)</param>
        /// <param name="zoomFactor">Hệ số zoom (1.0 = 100%)</param>
        /// <param name="rotationDegrees">Góc xoay (0, 90, 180, 270)</param>
        public BitmapSource? RenderPage(int pageIndex, double zoomFactor = 1.0, int rotationDegrees = 0)
        {
            if (_pdfDocument == null || pageIndex < 0 || pageIndex >= _pdfDocument.PageCount)
            {
                return null;
            }

            var pageSize = _pdfDocument.PageSizes[pageIndex];

            // Tăng DPI cơ sở để hình ảnh hiển thị sắc nét trên WPF DPI (144 DPI = 1.5x)
            double baseScale = 1.5;
            int width = (int)(pageSize.Width * baseScale * zoomFactor);
            int height = (int)(pageSize.Height * baseScale * zoomFactor);

            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            var rotation = (PdfRotation)((rotationDegrees / 90) % 4);

            using var bitmap = _pdfDocument.Render(pageIndex, width, height, 144, 144, rotation, PdfRenderFlags.Annotations);
            return ConvertBitmapToBitmapSource(bitmap);
        }

        private static BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Image bitmap)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze để an toàn luồng UI và MVVM

            return bitmapImage;
        }

        /// <summary>
        /// Trích xuất toàn bộ văn bản từ file PDF để sinh viên có thể bôi đen copy (Ctrl+C)
        /// </summary>
        public string GetAllText()
        {
            if (_pdfDocument == null) return string.Empty;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _pdfDocument.PageCount; i++)
            {
                sb.AppendLine($"--- Trang {i + 1} ---");
                try
                {
                    sb.AppendLine(_pdfDocument.GetPdfText(i));
                }
                catch { }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            if (_pdfDocument != null)
            {
                _pdfDocument.Dispose();
                _pdfDocument = null;
            }
            _currentFilePath = null;
        }
    }
}
