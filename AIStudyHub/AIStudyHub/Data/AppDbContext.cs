using Microsoft.EntityFrameworkCore;
using AIStudyHub.Models;

namespace AIStudyHub.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

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
    }
}
