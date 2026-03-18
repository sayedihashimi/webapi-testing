using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
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
        base.OnModelCreating(modelBuilder);

        // MembershipPlan
        modelBuilder.Entity<MembershipPlan>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.Email).IsUnique();
        });

        // Membership
        modelBuilder.Entity<Membership>(e =>
        {
            e.HasOne(m => m.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.MembershipPlan)
                .WithMany(p => p.Memberships)
                .HasForeignKey(m => m.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(m => m.Status).HasConversion<string>();
            e.Property(m => m.PaymentStatus).HasConversion<string>();
        });

        // Instructor
        modelBuilder.Entity<Instructor>(e =>
        {
            e.HasIndex(i => i.Email).IsUnique();
        });

        // ClassType
        modelBuilder.Entity<ClassType>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.DifficultyLevel).HasConversion<string>();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(e =>
        {
            e.HasOne(cs => cs.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(cs => cs.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(cs => cs.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(cs => cs.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(cs => cs.Status).HasConversion<string>();
        });

        // Booking
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasOne(b => b.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(b => b.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(b => b.Status).HasConversion<string>();
        });
    }
}
