using Microsoft.EntityFrameworkCore;
using AIStudyHub.Models;

namespace AIStudyHub.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<Annotation> Annotations { get; set; }

        public static void InitializeDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            // Khởi tạo các giá trị mặc định cho AppSetting nếu chưa có
            var defaultEndpoint = db.AppSettings.Find("ApiEndpoint");
            if (defaultEndpoint == null) db.AppSettings.Add(new AppSetting { Key = "ApiEndpoint", Value = "https://generativelanguage.googleapis.com/v1beta/models/" });

            var defaultModel = db.AppSettings.Find("AiModel");
            if (defaultModel == null) db.AppSettings.Add(new AppSetting { Key = "AiModel", Value = "gemma-4-31b-it" });

            db.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Tên file database sẽ được tạo ra tại thư mục chạy app
            optionsBuilder.UseSqlite("Data Source=AIStudyHub.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ép tên bảng viết hoa theo chuẩn SQL script
            modelBuilder.Entity<User>().ToTable("USER");
            modelBuilder.Entity<Subject>().ToTable("SUBJECT");
            modelBuilder.Entity<TaskItem>().ToTable("TASK");
            modelBuilder.Entity<Document>().ToTable("DOCUMENT");
            modelBuilder.Entity<ChatMessage>().ToTable("CHAT_MESSAGE");
            modelBuilder.Entity<DocumentChunk>().ToTable("DOCUMENT_CHUNK");
            modelBuilder.Entity<AppSetting>().ToTable("APP_SETTING");

            // Ràng buộc khoá ngoại (Cascade Delete)
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subjects)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task - Subject relationship
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Subject)
                .WithMany()
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Subject)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Document)
                .WithMany()
                .HasForeignKey(c => c.DocumentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentChunk>()
                .HasOne(c => c.Document)
                .WithMany()
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Đảm bảo bảng TASK tồn tại kể cả khi DB được tạo trước khi thêm TaskItem.
        /// Dùng CREATE TABLE IF NOT EXISTS để an toàn - không xóa dữ liệu cũ.
        /// </summary>
        public void EnsureTaskTableCreated()
        {
            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS TASK (
                    Id TEXT NOT NULL PRIMARY KEY,
                    SubjectId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    DueDate TEXT,
                    Status TEXT NOT NULL DEFAULT 'Todo',
                    Type TEXT NOT NULL DEFAULT 'Assignment',
                    CreatedAt TEXT NOT NULL,
                    CONSTRAINT FK_TASK_SUBJECT FOREIGN KEY (SubjectId)
                        REFERENCES SUBJECT(Id) ON DELETE CASCADE
                )
            ");
        }

        /// <summary>
        /// Đảm bảo bảng FLASHCARD và ANNOTATION tồn tại.
        /// </summary>
        public void EnsureFlashcardAndAnnotationTablesCreated()
        {
            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS FLASHCARD (
                    Id TEXT NOT NULL PRIMARY KEY,
                    DeckId TEXT NOT NULL,
                    FrontText TEXT NOT NULL,
                    BackText TEXT NOT NULL,
                    NextReviewDate TEXT,
                    RepetitionCount INTEGER NOT NULL DEFAULT 0,
                    EaseFactor REAL NOT NULL DEFAULT 2.5,
                    Interval INTEGER NOT NULL DEFAULT 0
                )
            ");

            Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ANNOTATION (
                    Id TEXT NOT NULL PRIMARY KEY,
                    DocumentId TEXT NOT NULL,
                    PageNumber INTEGER NOT NULL,
                    PosX REAL NOT NULL,
                    PosY REAL NOT NULL,
                    Content TEXT,
                    Type TEXT NOT NULL DEFAULT 'Highlight'
                )
            ");
        }
    }
}
