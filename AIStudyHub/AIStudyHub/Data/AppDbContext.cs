using Microsoft.EntityFrameworkCore;
using AIStudyHub.Models;

namespace AIStudyHub.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }

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

            // Ràng buộc khoá ngoại (Cascade Delete)
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subjects)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
