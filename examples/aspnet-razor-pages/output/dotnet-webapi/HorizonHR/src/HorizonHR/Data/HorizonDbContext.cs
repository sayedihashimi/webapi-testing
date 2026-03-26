using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public sealed class HorizonDbContext : DbContext
{
    public HorizonDbContext(DbContextOptions<HorizonDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department
        modelBuilder.Entity<Department>(e =>
        {
            e.HasIndex(d => d.Name).IsUnique();
            e.HasIndex(d => d.Code).IsUnique();

            e.HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(emp => emp.EmployeeNumber).IsUnique();
            e.HasIndex(emp => emp.Email).IsUnique();

            e.Property(emp => emp.Salary).HasColumnType("decimal(10,2)");
            e.Property(emp => emp.EmploymentType).HasConversion<string>();
            e.Property(emp => emp.Status).HasConversion<string>();

            e.HasOne(emp => emp.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(emp => emp.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(emp => emp.Manager)
                .WithMany(emp => emp.DirectReports)
                .HasForeignKey(emp => emp.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            e.Ignore(emp => emp.FullName);
        });

        // LeaveType
        modelBuilder.Entity<LeaveType>(e =>
        {
            e.HasIndex(lt => lt.Name).IsUnique();
        });

        // LeaveBalance
        modelBuilder.Entity<LeaveBalance>(e =>
        {
            e.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year }).IsUnique();
            e.Property(lb => lb.TotalDays).HasColumnType("decimal(5,1)");
            e.Property(lb => lb.UsedDays).HasColumnType("decimal(5,1)");
            e.Property(lb => lb.CarriedOverDays).HasColumnType("decimal(5,1)");

            e.HasOne(lb => lb.Employee)
                .WithMany(emp => emp.LeaveBalances)
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(lb => lb.LeaveType)
                .WithMany(lt => lt.LeaveBalances)
                .HasForeignKey(lb => lb.LeaveTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Ignore(lb => lb.RemainingDays);
        });

        // LeaveRequest
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.Property(lr => lr.TotalDays).HasColumnType("decimal(5,1)");
            e.Property(lr => lr.Status).HasConversion<string>();

            e.HasOne(lr => lr.Employee)
                .WithMany(emp => emp.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(lr => lr.LeaveType)
                .WithMany(lt => lt.LeaveRequests)
                .HasForeignKey(lr => lr.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.ReviewedBy)
                .WithMany()
                .HasForeignKey(lr => lr.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PerformanceReview
        modelBuilder.Entity<PerformanceReview>(e =>
        {
            e.Property(pr => pr.Status).HasConversion<string>();
            e.Property(pr => pr.OverallRating).HasConversion<string>();

            e.HasOne(pr => pr.Employee)
                .WithMany(emp => emp.PerformanceReviews)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pr => pr.Reviewer)
                .WithMany()
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Skill
        modelBuilder.Entity<Skill>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();
        });

        // EmployeeSkill
        modelBuilder.Entity<EmployeeSkill>(e =>
        {
            e.HasIndex(es => new { es.EmployeeId, es.SkillId }).IsUnique();
            e.Property(es => es.ProficiencyLevel).HasConversion<string>();

            e.HasOne(es => es.Employee)
                .WithMany(emp => emp.EmployeeSkills)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(es => es.Skill)
                .WithMany(s => s.EmployeeSkills)
                .HasForeignKey(es => es.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                entry.Property("UpdatedAt").CurrentValue = now;

            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") is not null)
                    entry.Property("CreatedAt").CurrentValue = now;

                if (entry.Metadata.FindProperty("SubmittedDate") is not null
                    && entry.Property("SubmittedDate").CurrentValue is DateTime dt && dt == default)
                    entry.Property("SubmittedDate").CurrentValue = now;
            }
        }
    }
}
