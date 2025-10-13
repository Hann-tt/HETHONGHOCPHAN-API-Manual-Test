using Microsoft.EntityFrameworkCore;
using HETHONGHOCPHAN.Models;

namespace HETHONGHOCPHAN.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor nhận options để truyền cấu hình DB
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Bảng Students
        public DbSet<Student> Students { get; set; }

        // 👉 Thêm bảng PasswordResets
        public DbSet<PasswordReset> PasswordResets { get; set; }

        // TODO: Bảng RegisteredCourses có thể thêm sau
        // public DbSet<RegisteredCourse> RegisteredCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasKey(s => s.StudentId);

            // Nếu muốn, có thể cấu hình thêm cho PasswordReset
            modelBuilder.Entity<PasswordReset>().HasKey(p => p.Id);
        }
    }
}
