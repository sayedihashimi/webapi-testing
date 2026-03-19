using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<ClassType> ClassTypes => Set<ClassType>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MembershipPlan>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<Membership>(e =>
        {
            e.HasOne(m => m.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.MembershipPlan)
                .WithMany()
                .HasForeignKey(m => m.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(m => m.Status).HasConversion<string>();
            e.Property(m => m.PaymentStatus).HasConversion<string>();
        });

        modelBuilder.Entity<Instructor>(e =>
        {
            e.HasIndex(i => i.Email).IsUnique();
        });

        modelBuilder.Entity<ClassType>(e =>
        {
            e.HasIndex(ct => ct.Name).IsUnique();
            e.Property(ct => ct.DifficultyLevel).HasConversion<string>();
        });

        modelBuilder.Entity<ClassSchedule>(e =>
        {
            e.HasOne(cs => cs.ClassType)
                .WithMany()
                .HasForeignKey(cs => cs.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(cs => cs.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(cs => cs.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(cs => cs.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasOne(b => b.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(b => b.ClassScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(b => b.Status).HasConversion<string>();
        });
    }
}
