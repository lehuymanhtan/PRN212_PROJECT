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
        /// Upload file PDF hoặc Word từ máy vào thư mục App Data và lưu đường dẫn vào DB
        /// </summary>
        public async Task<Document> UploadDocumentAsync(string sourceFilePath, Guid subjectId, string? customTitle = null)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Không tìm thấy file nguồn.", sourceFilePath);
            }

            var fileExtension = Path.GetExtension(sourceFilePath).ToUpperInvariant();
            string fileType = fileExtension switch
            {
                ".PDF" => "PDF",
                ".DOCX" => "DOCX",
                ".DOC" => "DOC",
                _ => fileExtension.TrimStart('.')
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
                SubjectId = subjectId,
                Title = title,
                FilePath = destinationFilePath,
                FileType = fileType,
                UploadedAt = DateTime.Now
            };

            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync();

            return document;
        }

        /// <summary>
        /// Lấy danh sách tài liệu theo môn học hoặc toàn bộ
        /// </summary>
        public List<Document> GetDocuments(Guid? subjectId = null)
        {
            using var dbContext = new AppDbContext();
            var query = dbContext.Documents.Include(d => d.Subject).AsQueryable();

            if (subjectId.HasValue && subjectId.Value != Guid.Empty)
            {
                query = query.Where(d => d.SubjectId == subjectId.Value);
            }

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
                {
                    File.Delete(document.FilePath);
                }
            }
            catch
            {
                // Bỏ qua lỗi nếu file đang mở hoặc bị khóa
            }
        }
    }
}
