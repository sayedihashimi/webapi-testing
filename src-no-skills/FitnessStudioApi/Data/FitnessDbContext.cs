using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Data;

public class FitnessDbContext : DbContext
{
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }

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
        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Membership
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MembershipPlan)
                .WithMany(p => p.Memberships)
                .HasForeignKey(e => e.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ClassType
        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasOne(e => e.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(e => e.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(e => e.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(e => e.ClassScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(e => e.MemberId)
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
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is MembershipPlan mp) mp.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Member m) m.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Membership ms) ms.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Instructor i) i.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is ClassType ct) ct.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is ClassSchedule cs) cs.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Booking b) b.UpdatedAt = DateTime.UtcNow;
        }
    }
}
