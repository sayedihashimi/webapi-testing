using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options) : DbContext(options)
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
            e.Property(p => p.Name).HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.Email).IsUnique();
            e.Property(m => m.FirstName).HasMaxLength(100);
            e.Property(m => m.LastName).HasMaxLength(100);
            e.Property(m => m.EmergencyContactName).HasMaxLength(200);
        });

        // Membership
        modelBuilder.Entity<Membership>(e =>
        {
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(m => m.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.Member)
                .WithMany(mb => mb.Memberships)
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.MembershipPlan)
                .WithMany(p => p.Memberships)
                .HasForeignKey(m => m.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(e =>
        {
            e.HasIndex(i => i.Email).IsUnique();
            e.Property(i => i.FirstName).HasMaxLength(100);
            e.Property(i => i.LastName).HasMaxLength(100);
            e.Property(i => i.Bio).HasMaxLength(1000);
        });

        // ClassType
        modelBuilder.Entity<ClassType>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.DifficultyLevel).HasConversion<string>().HasMaxLength(20);
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(e =>
        {
            e.Property(c => c.Room).HasMaxLength(50);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(c => c.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(c => c.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(e =>
        {
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(b => b.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(b => b.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(b => new { b.ClassScheduleId, b.MemberId, b.Status });
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Membership Plans
        modelBuilder.Entity<MembershipPlan>().HasData(
            new MembershipPlan { Id = 1, Name = "Basic", Description = "Access to standard classes, 3 bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 2, Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week", DurationMonths = 3, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 3, Name = "Elite", Description = "Unlimited access to all classes and premium features", DurationMonths = 12, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Members
        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = new DateOnly(2024, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Li Chen", EmergencyContactPhone = "555-0202", JoinDate = new DateOnly(2024, 7, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 3, FirstName = "Sarah", LastName = "Williams", Email = "sarah.williams@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Tom Williams", EmergencyContactPhone = "555-0302", JoinDate = new DateOnly(2024, 8, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 4, FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Grace Kim", EmergencyContactPhone = "555-0402", JoinDate = new DateOnly(2024, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 5, FirstName = "Emma", LastName = "Rodriguez", Email = "emma.rodriguez@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0502", JoinDate = new DateOnly(2024, 10, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 6, FirstName = "James", LastName = "Patel", Email = "james.patel@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1991, 9, 25), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0602", JoinDate = new DateOnly(2024, 10, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 7, FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1993, 4, 18), EmergencyContactName = "Michael Brown", EmergencyContactPhone = "555-0702", JoinDate = new DateOnly(2024, 11, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 8, FirstName = "Liam", LastName = "Nguyen", Email = "liam.nguyen@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(2000, 12, 5), EmergencyContactName = "Thi Nguyen", EmergencyContactPhone = "555-0802", JoinDate = new DateOnly(2024, 11, 15), IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // Instructors
        modelBuilder.Entity<Instructor>().HasData(
            new Instructor { Id = 1, FirstName = "Maya", LastName = "Thompson", Email = "maya.thompson@zenith.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years experience", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2022, 1, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 2, FirstName = "Ryan", LastName = "Garcia", Email = "ryan.garcia@zenith.com", Phone = "555-1002", Bio = "Former competitive athlete specializing in high-intensity training", Specializations = "HIIT,Boxing,Spin", HireDate = new DateOnly(2022, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 3, FirstName = "Sofia", LastName = "Lee", Email = "sofia.lee@zenith.com", Phone = "555-1003", Bio = "Pilates and barre specialist with rehabilitation expertise", Specializations = "Pilates,Yoga", HireDate = new DateOnly(2023, 3, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 4, FirstName = "Tyler", LastName = "Jackson", Email = "tyler.jackson@zenith.com", Phone = "555-1004", Bio = "Spinning and cardio expert, marathon runner", Specializations = "Spin,HIIT", HireDate = new DateOnly(2023, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // ClassTypes
        modelBuilder.Entity<ClassType>().HasData(
            new ClassType { Id = 1, Name = "Yoga", Description = "Vinyasa flow yoga for all levels", DefaultDurationMinutes = 60, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 2, Name = "HIIT", Description = "High-intensity interval training to burn calories fast", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 3, Name = "Spin", Description = "Indoor cycling class with energetic music", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 4, Name = "Pilates", Description = "Core-focused Pilates with reformer equipment", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 5, Name = "Boxing", Description = "Boxing fitness combining technique and cardio", DefaultDurationMinutes = 60, DefaultCapacity = 16, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 6, Name = "Meditation", Description = "Guided meditation and mindfulness practice", DefaultDurationMinutes = 30, DefaultCapacity = 30, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );
    }
}
