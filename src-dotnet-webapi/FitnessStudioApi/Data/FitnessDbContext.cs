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
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
            e.Property(m => m.LastName).IsRequired().HasMaxLength(100);
            e.Property(m => m.Email).IsRequired();
            e.HasIndex(m => m.Email).IsUnique();
            e.Property(m => m.Phone).IsRequired();
            e.Property(m => m.EmergencyContactName).IsRequired().HasMaxLength(200);
            e.Property(m => m.EmergencyContactPhone).IsRequired();
        });

        // Membership
        modelBuilder.Entity<Membership>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Status).HasConversion<string>();
            e.Property(m => m.PaymentStatus).HasConversion<string>();
            e.HasOne(m => m.Member).WithMany(m => m.Memberships).HasForeignKey(m => m.MemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.MembershipPlan).WithMany(p => p.Memberships).HasForeignKey(m => m.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.FirstName).IsRequired().HasMaxLength(100);
            e.Property(i => i.LastName).IsRequired().HasMaxLength(100);
            e.Property(i => i.Email).IsRequired();
            e.HasIndex(i => i.Email).IsUnique();
            e.Property(i => i.Phone).IsRequired();
            e.Property(i => i.Bio).HasMaxLength(1000);
        });

        // ClassType
        modelBuilder.Entity<ClassType>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.DifficultyLevel).HasConversion<string>();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Room).IsRequired().HasMaxLength(50);
            e.Property(c => c.Status).HasConversion<string>();
            e.HasOne(c => c.ClassType).WithMany(ct => ct.ClassSchedules).HasForeignKey(c => c.ClassTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Instructor).WithMany(i => i.ClassSchedules).HasForeignKey(c => c.InstructorId).OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Status).HasConversion<string>();
            e.HasOne(b => b.ClassSchedule).WithMany(cs => cs.Bookings).HasForeignKey(b => b.ClassScheduleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Member).WithMany(m => m.Bookings).HasForeignKey(b => b.MemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(b => new { b.ClassScheduleId, b.MemberId }).IsUnique().HasFilter("\"Status\" != 'Cancelled'");
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
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Member m) { m.CreatedAt = now; m.UpdatedAt = now; }
                else if (entry.Entity is MembershipPlan mp) { mp.CreatedAt = now; mp.UpdatedAt = now; }
                else if (entry.Entity is Membership ms) { ms.CreatedAt = now; ms.UpdatedAt = now; }
                else if (entry.Entity is Instructor i) { i.CreatedAt = now; i.UpdatedAt = now; }
                else if (entry.Entity is ClassType ct) { ct.CreatedAt = now; ct.UpdatedAt = now; }
                else if (entry.Entity is ClassSchedule cs) { cs.CreatedAt = now; cs.UpdatedAt = now; }
                else if (entry.Entity is Booking b) { b.CreatedAt = now; b.UpdatedAt = now; }
            }
            else
            {
                if (entry.Entity is Member m) m.UpdatedAt = now;
                else if (entry.Entity is MembershipPlan mp) mp.UpdatedAt = now;
                else if (entry.Entity is Membership ms) ms.UpdatedAt = now;
                else if (entry.Entity is Instructor i) i.UpdatedAt = now;
                else if (entry.Entity is ClassType ct) ct.UpdatedAt = now;
                else if (entry.Entity is ClassSchedule cs) cs.UpdatedAt = now;
                else if (entry.Entity is Booking b) b.UpdatedAt = now;
            }
        }
    }
}
