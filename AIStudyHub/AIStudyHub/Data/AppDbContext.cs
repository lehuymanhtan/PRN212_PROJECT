using Microsoft.EntityFrameworkCore;
using AIStudyHub.Models;

namespace AIStudyHub.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Document> Documents { get; set; }

        public static void InitializeDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            // Đảm bảo tạo bảng DOCUMENT nếu database cũ trước đó chưa có bảng này
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""DOCUMENT"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_DOCUMENT"" PRIMARY KEY,
                    ""SubjectId"" TEXT NOT NULL,
                    ""Title"" TEXT NOT NULL,
                    ""FilePath"" TEXT NOT NULL,
                    ""FileType"" TEXT NULL,
                    ""UploadedAt"" TEXT NOT NULL,
                    CONSTRAINT ""FK_DOCUMENT_SUBJECT_SubjectId"" FOREIGN KEY (""SubjectId"") REFERENCES ""SUBJECT"" (""Id"") ON DELETE CASCADE
                );
            ");
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
            modelBuilder.Entity<Document>().ToTable("DOCUMENT");

            // Ràng buộc khoá ngoại (Cascade Delete)
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subjects)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Subject)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
