using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
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

            e.HasOne(emp => emp.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(emp => emp.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(emp => emp.Manager)
                .WithMany(emp => emp.DirectReports)
                .HasForeignKey(emp => emp.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
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
        });

        // LeaveRequest
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasOne(lr => lr.Employee)
                .WithMany(emp => emp.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.ReviewedBy)
                .WithMany()
                .HasForeignKey(lr => lr.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PerformanceReview
        modelBuilder.Entity<PerformanceReview>(e =>
        {
            e.HasOne(pr => pr.Employee)
                .WithMany(emp => emp.PerformanceReviews)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(pr => pr.Reviewer)
                .WithMany()
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeSkill
        modelBuilder.Entity<EmployeeSkill>(e =>
        {
            e.HasIndex(es => new { es.EmployeeId, es.SkillId }).IsUnique();
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
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Department d) { d.CreatedAt = now; d.UpdatedAt = now; }
                if (entry.Entity is Employee e) { e.CreatedAt = now; e.UpdatedAt = now; }
                if (entry.Entity is LeaveRequest lr) { lr.CreatedAt = now; lr.UpdatedAt = now; lr.SubmittedDate = now; }
                if (entry.Entity is PerformanceReview pr) { pr.CreatedAt = now; pr.UpdatedAt = now; }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Department d) { d.UpdatedAt = now; }
                if (entry.Entity is Employee e) { e.UpdatedAt = now; }
                if (entry.Entity is LeaveRequest lr) { lr.UpdatedAt = now; }
                if (entry.Entity is PerformanceReview pr) { pr.UpdatedAt = now; }
            }
        }
    }
}
