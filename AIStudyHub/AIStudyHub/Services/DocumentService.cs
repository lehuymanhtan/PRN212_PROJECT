using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AIStudyHub.Data;
using AIStudyHub.Models;

namespace AIStudyHub.Services
{
    public class DocumentService
    {
        private readonly string _storageBaseFolder;

        public DocumentService()
        {
            // Thư mục lưu trữ App Data theo yêu cầu của Thành viên 4
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _storageBaseFolder = Path.Combine(appDataPath, "AIStudyHub", "Documents");
            Directory.CreateDirectory(_storageBaseFolder);
        }

        /// <summary>
        /// Upload file từ máy vào thư mục App Data và lưu đường dẫn vào DB
        /// Hỗ trợ: PDF, DOCX, DOC, PPTX, PPT, XLSX, XLS, TXT, MD
        /// </summary>
        public async Task<Document> UploadDocumentAsync(string sourceFilePath, Guid subjectId, string? customTitle = null)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Không tìm thấy file nguồn.", sourceFilePath);

            var fileExtension = Path.GetExtension(sourceFilePath).ToUpperInvariant();
            string fileType = fileExtension switch
            {
                ".PDF"  => "PDF",
                ".DOCX" => "DOCX",
                ".DOC"  => "DOC",
                ".PPTX" => "PPTX",
                ".PPT"  => "PPT",
                ".XLSX" => "XLSX",
                ".XLS"  => "XLS",
                ".TXT"  => "TXT",
                ".MD"   => "MD",
                _       => fileExtension.TrimStart('.')
            };

            var originalFileName = Path.GetFileName(sourceFilePath);
            var title = !string.IsNullOrWhiteSpace(customTitle)
                ? customTitle
                : Path.GetFileNameWithoutExtension(originalFileName);

            // Tạo thư mục riêng theo môn học để dễ quản lý
            var subjectFolder = Path.Combine(_storageBaseFolder, subjectId.ToString());
            Directory.CreateDirectory(subjectFolder);

            // Đảm bảo tên file không bị trùng lặp
            var newFileName = $"{Guid.NewGuid():N}_{originalFileName}";
            var destinationFilePath = Path.Combine(subjectFolder, newFileName);

            // Copy file từ máy vào App Data
            await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite: true));

            // Lưu thông tin vào Database
            using var dbContext = new AppDbContext();
            var document = new Document
            {
                SubjectId   = subjectId,
                Title       = title,
                FilePath    = destinationFilePath,
                FileType    = fileType,
                UploadedAt  = DateTime.Now
            };

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();

            // RAG Chunking: Trích xuất và băm văn bản sau khi lưu
            try
            {
                var fullText = ExtractText(destinationFilePath, fileType);
                if (!string.IsNullOrWhiteSpace(fullText))
                {
                    var chunks = SplitIntoChunks(fullText, 1000);
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        dbContext.DocumentChunks.Add(new DocumentChunk
                        {
                            DocumentId = document.Id,
                            ChunkIndex = i,
                            Content    = chunks[i]
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi Chunking nếu có để không làm gián đoạn việc upload
            }

            return document;
        }

        /// <summary>
        /// Trích xuất văn bản thuần từ file dựa theo loại file.
        /// </summary>
        private static string ExtractText(string filePath, string fileType)
        {
            switch (fileType.ToUpperInvariant())
            {
                case "PDF":
                {
                    using var pdfViewer = new PdfViewerService();
                    pdfViewer.LoadPdf(filePath);
                    return pdfViewer.GetAllText();
                }

                case "DOCX":
                case "DOC":
                {
                    var converter = new Mammoth.DocumentConverter();
                    return converter.ExtractRawText(filePath).Value;
                }

                case "PPTX":
                {
                    // DocumentFormat.OpenXml for PPTX text extraction
                    using var prs = DocumentFormat.OpenXml.Packaging.PresentationDocument.Open(filePath, false);
                    var sb           = new System.Text.StringBuilder();
                    var presentation = prs.PresentationPart?.Presentation;
                    if (presentation?.SlideIdList != null)
                    {
                        foreach (var slideId in presentation.SlideIdList
                            .Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
                        {
                            var relId = slideId.RelationshipId?.Value;
                            if (relId == null) continue;
                            var slidePart = (DocumentFormat.OpenXml.Packaging.SlidePart)
                                prs.PresentationPart!.GetPartById(relId);
                            foreach (var para in slidePart.Slide
                                .Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                            {
                                var line = string.Concat(
                                    para.Descendants<DocumentFormat.OpenXml.Drawing.Run>()
                                        .Select(r => r.Text?.Text ?? string.Empty));
                                if (!string.IsNullOrWhiteSpace(line))
                                    sb.AppendLine(line);
                            }
                        }
                    }
                    return sb.ToString();
                }

                case "PPT":
                    // Legacy binary PPT — NPOI HSLF not available for .NET 8.
                    // Summarization still works via Gemini File API upload.
                    return string.Empty;

                case "XLSX":
                {
                    using var stream = File.OpenRead(filePath);
                    var wb = new NPOI.XSSF.UserModel.XSSFWorkbook(stream);
                    var sb = new System.Text.StringBuilder();
                    for (int si = 0; si < wb.NumberOfSheets; si++)
                    {
                        var sheet = wb.GetSheetAt(si);
                        for (int r = 0; r <= sheet.LastRowNum; r++)
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            for (int c = 0; c < row.LastCellNum; c++)
                            {
                                var cell = row.GetCell(c);
                                if (cell != null) sb.Append(cell.ToString()).Append('\t');
                            }
                            sb.AppendLine();
                        }
                    }
                    return sb.ToString();
                }

                case "XLS":
                {
                    using var stream = File.OpenRead(filePath);
                    var wb = new NPOI.HSSF.UserModel.HSSFWorkbook(stream);
                    var sb = new System.Text.StringBuilder();
                    for (int si = 0; si < wb.NumberOfSheets; si++)
                    {
                        var sheet = wb.GetSheetAt(si);
                        for (int r = 0; r <= sheet.LastRowNum; r++)
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            for (int c = 0; c < row.LastCellNum; c++)
                            {
                                var cell = row.GetCell(c);
                                if (cell != null) sb.Append(cell.ToString()).Append('\t');
                            }
                            sb.AppendLine();
                        }
                    }
                    return sb.ToString();
                }

                case "TXT":
                case "MD":
                    return File.ReadAllText(filePath);

                default:
                    return string.Empty;
            }
        }

        private static List<string> SplitIntoChunks(string text, int chunkSize)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize)
                chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
            return chunks;
        }

        /// <summary>
        /// Lấy danh sách tài liệu theo môn học hoặc toàn bộ
        /// </summary>
        public List<Document> GetDocuments(Guid? subjectId = null)
        {
            using var dbContext = new AppDbContext();
            var query = dbContext.Documents.Include(d => d.Subject).AsQueryable();

            if (subjectId.HasValue && subjectId.Value != Guid.Empty)
                query = query.Where(d => d.SubjectId == subjectId.Value);

            return query.OrderByDescending(d => d.UploadedAt).ToList();
        }

        /// <summary>
        /// Xóa tài liệu khỏi DB và xóa file lưu trữ trong App Data
        /// </summary>
        public void DeleteDocument(Document document)
        {
            if (document == null) return;

            using var dbContext = new AppDbContext();
            var existing = dbContext.Documents.Find(document.Id);
            if (existing != null)
            {
                dbContext.Documents.Remove(existing);
                dbContext.SaveChanges();
            }

            try
            {
                if (File.Exists(document.FilePath))
                    File.Delete(document.FilePath);
            }
            catch
            {
                // Bỏ qua lỗi nếu file đang mở hoặc bị khóa
            }
        }
    }
}
