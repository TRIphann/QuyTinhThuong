using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models
{
    public class QLQuyTinhThuongContext : DbContext
    {
        public QLQuyTinhThuongContext(DbContextOptions<QLQuyTinhThuongContext> options)
            : base(options)
        {
        }

        // ========================
        // Bảng hệ thống quỹ tình thương
        // ========================
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Log> Logs { get; set; }

        // ========================
        // Cấu hình quan hệ
        // ========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính phức hợp cho User_Roles
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // User - UserRole (1-n)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role - UserRole (1-n)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Donor - Donation (1-n)
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Donor)
                .WithMany(donor => donor.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.NoAction);

            // User - Donation (1-n) - ReceivedBy
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.ReceivedByUser)
                .WithMany(u => u.DonationsReceived)
                .HasForeignKey(d => d.ReceivedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Beneficiary - SupportRequest (1-n)
            modelBuilder.Entity<SupportRequest>()
                .HasOne(sr => sr.Beneficiary)
                .WithMany(b => b.SupportRequests)
                .HasForeignKey(sr => sr.BeneficiaryId)
                .OnDelete(DeleteBehavior.NoAction);

            // SupportRequest - Approval (1-n)
            modelBuilder.Entity<Approval>()
                .HasOne(a => a.SupportRequest)
                .WithMany(sr => sr.Approvals)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // User - Approval (1-n) - ApprovedBy
            modelBuilder.Entity<Approval>()
                .HasOne(a => a.ApprovedByUser)
                .WithMany(u => u.Approvals)
                .HasForeignKey(a => a.ApprovedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // SupportRequest - Expense (1-n)
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.SupportRequest)
                .WithMany(sr => sr.Expenses)
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // User - Expense (1-n) - PaidBy
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidByUser)
                .WithMany(u => u.ExpensesPaid)
                .HasForeignKey(e => e.PaidBy)
                .OnDelete(DeleteBehavior.SetNull);

            // User - Log (1-n)
            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
