using LecturerClaimsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LecturerClaimsSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<PaymentReport> PaymentReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>(entity =>
            {
                entity.Property(c => c.Lecturer).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Status).IsRequired().HasMaxLength(20);
                entity.Property(c => c.ApprovedBy).HasMaxLength(100);
                entity.Property(c => c.Notes).HasMaxLength(1000);
                entity.Property(c => c.DocumentPath).HasMaxLength(500);
            });

            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.Property(l => l.EmployeeId).IsRequired().HasMaxLength(20);
                entity.Property(l => l.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(l => l.LastName).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Email).IsRequired().HasMaxLength(100);
                entity.Property(l => l.PhoneNumber).HasMaxLength(20);
                entity.Property(l => l.Department).HasMaxLength(100);
                entity.Property(l => l.Position).HasMaxLength(100);
                entity.HasIndex(l => l.EmployeeId).IsUnique();
                entity.HasIndex(l => l.Email).IsUnique();
            });

            modelBuilder.Entity<PaymentReport>(entity =>
            {
                entity.Property(pr => pr.ReportName).IsRequired().HasMaxLength(200);
                entity.Property(pr => pr.GeneratedBy).IsRequired().HasMaxLength(100);
            });
        }
    }
}