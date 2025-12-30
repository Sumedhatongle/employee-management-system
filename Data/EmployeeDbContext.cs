using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Punch> Punches { get; set; }
        public DbSet<Leave> Leaves { get; set; }

        // Views
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<PunchSummary> PunchSummaries { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Employee)
                      .HasForeignKey<Employee>(e => e.UserId);
            });

            // Punch configuration
            modelBuilder.Entity<Punch>(entity =>
            {
                entity.HasKey(e => e.PunchId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Punches)
                      .HasForeignKey(e => e.EmployeeId);
                entity.Property(e => e.PunchType).HasConversion<string>();
            });

            // Leave configuration
            modelBuilder.Entity<Leave>(entity =>
            {
                entity.HasKey(e => e.LeaveId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Leaves)
                      .HasForeignKey(e => e.EmployeeId);
                entity.Property(e => e.LeaveType).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
            });

            // Views configuration
            modelBuilder.Entity<EmployeeProfile>().HasNoKey().ToView("vw_EmployeeProfile");
            modelBuilder.Entity<PunchSummary>().HasNoKey().ToView("vw_PunchSummary");
            modelBuilder.Entity<LeaveRequest>().HasNoKey().ToView("vw_LeaveRequests");
        }
    }

    // View Models
    public class EmployeeProfile
    {
        public long EmployeeId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
    }

    public class PunchSummary
    {
        public long EmployeeId { get; set; }
        public DateTime PunchDate { get; set; }
        public DateTime? FirstIn { get; set; }
        public DateTime? LastOut { get; set; }
    }

    public class LeaveRequest
    {
        public long LeaveId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; }
    }
}