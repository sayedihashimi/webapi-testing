using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
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

        ConfigureMembershipPlan(modelBuilder);
        ConfigureMember(modelBuilder);
        ConfigureMembership(modelBuilder);
        ConfigureInstructor(modelBuilder);
        ConfigureClassType(modelBuilder);
        ConfigureClassSchedule(modelBuilder);
        ConfigureBooking(modelBuilder);

        SeedData(modelBuilder);
    }

    private static void ConfigureMembershipPlan(ModelBuilder mb)
    {
        mb.Entity<MembershipPlan>(e =>
        {
            e.HasIndex(p => p.Name).IsUnique();
            e.Property(p => p.Name).HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        });
    }

    private static void ConfigureMember(ModelBuilder mb)
    {
        mb.Entity<Member>(e =>
        {
            e.HasIndex(m => m.Email).IsUnique();
            e.Property(m => m.FirstName).HasMaxLength(100);
            e.Property(m => m.LastName).HasMaxLength(100);
            e.Property(m => m.Email).HasMaxLength(200);
            e.Property(m => m.Phone).HasMaxLength(20);
            e.Property(m => m.EmergencyContactName).HasMaxLength(200);
            e.Property(m => m.EmergencyContactPhone).HasMaxLength(20);
        });
    }

    private static void ConfigureMembership(ModelBuilder mb)
    {
        mb.Entity<Membership>(e =>
        {
            e.Property(m => m.Status).HasConversion<string>();
            e.Property(m => m.PaymentStatus).HasConversion<string>();

            e.HasOne(m => m.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(m => m.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.MembershipPlan)
                .WithMany(p => p.Memberships)
                .HasForeignKey(m => m.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInstructor(ModelBuilder mb)
    {
        mb.Entity<Instructor>(e =>
        {
            e.HasIndex(i => i.Email).IsUnique();
            e.Property(i => i.FirstName).HasMaxLength(100);
            e.Property(i => i.LastName).HasMaxLength(100);
            e.Property(i => i.Email).HasMaxLength(200);
            e.Property(i => i.Phone).HasMaxLength(20);
            e.Property(i => i.Bio).HasMaxLength(1000);
            e.Property(i => i.Specializations).HasMaxLength(500);
        });
    }

    private static void ConfigureClassType(ModelBuilder mb)
    {
        mb.Entity<ClassType>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.DifficultyLevel).HasConversion<string>();
        });
    }

    private static void ConfigureClassSchedule(ModelBuilder mb)
    {
        mb.Entity<ClassSchedule>(e =>
        {
            e.Property(c => c.Room).HasMaxLength(50);
            e.Property(c => c.Status).HasConversion<string>();
            e.Property(c => c.CancellationReason).HasMaxLength(500);

            e.HasOne(c => c.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(c => c.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBooking(ModelBuilder mb)
    {
        mb.Entity<Booking>(e =>
        {
            e.Property(b => b.Status).HasConversion<string>();
            e.Property(b => b.CancellationReason).HasMaxLength(500);

            e.HasOne(b => b.ClassSchedule)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder mb)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Membership Plans
        mb.Entity<MembershipPlan>().HasData(
            new MembershipPlan { Id = 1, Name = "Basic", Description = "Access to standard classes, 3 bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 2, Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 3, Name = "Elite", Description = "Unlimited access to all classes and premium features", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Members
        mb.Entity<Member>().HasData(
            new Member { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = new DateOnly(2024, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Li Chen", EmergencyContactPhone = "555-0202", JoinDate = new DateOnly(2024, 7, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 3, FirstName = "Sophia", LastName = "Martinez", Email = "sophia.martinez@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0302", JoinDate = new DateOnly(2024, 8, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0402", JoinDate = new DateOnly(2024, 5, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 5, FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Tom Davis", EmergencyContactPhone = "555-0502", JoinDate = new DateOnly(2024, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 6, FirstName = "Daniel", LastName = "Brown", Email = "daniel.brown@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Lisa Brown", EmergencyContactPhone = "555-0602", JoinDate = new DateOnly(2024, 4, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 7, FirstName = "Olivia", LastName = "Wilson", Email = "olivia.wilson@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(2000, 2, 14), EmergencyContactName = "Mark Wilson", EmergencyContactPhone = "555-0702", JoinDate = new DateOnly(2024, 10, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 8, FirstName = "Ryan", LastName = "Taylor", Email = "ryan.taylor@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 12, 3), EmergencyContactName = "Karen Taylor", EmergencyContactPhone = "555-0802", JoinDate = new DateOnly(2024, 3, 1), IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // Memberships - active, expired, and one cancelled
        mb.Entity<Membership>().HasData(
            new Membership { Id = 1, MemberId = 1, MembershipPlanId = 2, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 2, MemberId = 2, MembershipPlanId = 3, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired memberships
            new Membership { Id = 7, MemberId = 1, MembershipPlanId = 1, StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2024, 7, 1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 8, MemberId = 8, MembershipPlanId = 1, StartDate = new DateOnly(2024, 3, 1), EndDate = new DateOnly(2024, 4, 1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now }
        );

        // Instructors
        mb.Entity<Instructor>().HasData(
            new Instructor { Id = 1, FirstName = "Sarah", LastName = "Kim", Email = "sarah.kim@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2020, 1, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 2, FirstName = "Mike", LastName = "Torres", Email = "mike.torres@zenithfitness.com", Phone = "555-1002", Bio = "Former competitive athlete specializing in high-intensity training", Specializations = "HIIT,Boxing,Spin", HireDate = new DateOnly(2021, 3, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 3, FirstName = "Priya", LastName = "Patel", Email = "priya.patel@zenithfitness.com", Phone = "555-1003", Bio = "Pilates and mindfulness expert with a holistic approach", Specializations = "Pilates,Yoga,Meditation", HireDate = new DateOnly(2022, 6, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 4, FirstName = "David", LastName = "Okonkwo", Email = "david.okonkwo@zenithfitness.com", Phone = "555-1004", Bio = "Professional boxing coach and strength training specialist", Specializations = "Boxing,HIIT,Spin", HireDate = new DateOnly(2023, 1, 10), IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Class Types
        mb.Entity<ClassType>().HasData(
            new ClassType { Id = 1, Name = "Yoga", Description = "Vinyasa flow yoga for flexibility and mindfulness", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 2, Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 3, Name = "Spin", Description = "Indoor cycling for cardiovascular endurance", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 4, Name = "Pilates", Description = "Core strengthening and body alignment", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 5, Name = "Boxing", Description = "Boxing fundamentals and cardio boxing workout", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 6, Name = "Meditation", Description = "Guided meditation and breathing exercises for stress relief", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Class Schedules - use relative dates approach with a base date
        // We seed with fixed dates in the future. In a real app, you'd use dynamic scheduling.
        var baseDate = new DateTime(2025, 7, 7, 0, 0, 0, DateTimeKind.Utc); // A Monday

        mb.Entity<ClassSchedule>().HasData(
            new ClassSchedule { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddHours(8), EndTime = baseDate.AddHours(9), Capacity = 20, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddHours(10), EndTime = baseDate.AddHours(10).AddMinutes(45), Capacity = 15, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio B", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 3, ClassTypeId = 3, InstructorId = 2, StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(7).AddMinutes(45), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Spin Room", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 4, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(10), Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio A", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 5, ClassTypeId = 5, InstructorId = 4, StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(18), Capacity = 10, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Boxing Ring", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 6, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(2).AddHours(19), EndTime = baseDate.AddDays(2).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Zen Room", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 7, ClassTypeId = 1, InstructorId = 3, StartTime = baseDate.AddDays(3).AddHours(8), EndTime = baseDate.AddDays(3).AddHours(9), Capacity = 20, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 8, ClassTypeId = 2, InstructorId = 4, StartTime = baseDate.AddDays(3).AddHours(18), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(45), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 9, ClassTypeId = 3, InstructorId = 2, StartTime = baseDate.AddDays(4).AddHours(7), EndTime = baseDate.AddDays(4).AddHours(7).AddMinutes(45), Capacity = 12, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Spin Room", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 10, ClassTypeId = 5, InstructorId = 4, StartTime = baseDate.AddDays(4).AddHours(17), EndTime = baseDate.AddDays(4).AddHours(18), Capacity = 10, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Boxing Ring", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Full class with waitlist
            new ClassSchedule { Id = 11, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(5).AddHours(9), EndTime = baseDate.AddDays(5).AddHours(10), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Studio A", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 12, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(5).AddHours(19), EndTime = baseDate.AddDays(5).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Zen Room", Status = ClassStatus.Scheduled, CreatedAt = now, UpdatedAt = now }
        );

        // Bookings
        mb.Entity<Booking>().HasData(
            // Class 1 (Yoga Mon 8am) - 3 confirmed
            new Booking { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 3, ClassScheduleId = 1, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 2 (HIIT Mon 10am) - 3 bookings
            new Booking { Id = 4, ClassScheduleId = 2, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 5, ClassScheduleId = 2, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 6, ClassScheduleId = 2, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 3 (Spin Tue 7am) - 2 confirmed
            new Booking { Id = 7, ClassScheduleId = 3, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 8, ClassScheduleId = 3, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 4 (Pilates Tue 9am) - 2 confirmed (premium)
            new Booking { Id = 9, ClassScheduleId = 4, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 10, ClassScheduleId = 4, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 5 (Boxing Wed 5pm) - 2 confirmed (premium)
            new Booking { Id = 11, ClassScheduleId = 5, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 12, ClassScheduleId = 5, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 6 (Meditation Wed 7pm)
            new Booking { Id = 13, ClassScheduleId = 6, MemberId = 7, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 7 (Yoga Thu 8am)
            new Booking { Id = 14, ClassScheduleId = 7, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 8 (HIIT Thu 6pm)
            new Booking { Id = 15, ClassScheduleId = 8, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 11 (Pilates Sat 9am - FULL, capacity=3) - 3 confirmed + 2 waitlisted
            new Booking { Id = 16, ClassScheduleId = 11, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 17, ClassScheduleId = 11, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 18, ClassScheduleId = 11, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 19, ClassScheduleId = 11, MemberId = 6, BookingDate = now, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 20, ClassScheduleId = 11, MemberId = 3, BookingDate = now, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, CreatedAt = now, UpdatedAt = now },
            // Class 12 (Meditation Sat 7pm)
            new Booking { Id = 21, ClassScheduleId = 12, MemberId = 7, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now }
        );
    }
}
