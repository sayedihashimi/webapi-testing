using FitnessStudioApi.Models;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static void Seed(FitnessDbContext db)
    {
        if (db.MembershipPlans.Any()) return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basic = new MembershipPlan
        {
            Name = "Basic",
            Description = "Access to standard classes with limited bookings per week",
            DurationMonths = 1,
            Price = 29.99m,
            MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        var premium = new MembershipPlan
        {
            Name = "Premium",
            Description = "Access to all classes including premium, with more bookings per week",
            DurationMonths = 1,
            Price = 49.99m,
            MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var elite = new MembershipPlan
        {
            Name = "Elite",
            Description = "Unlimited access to all classes, priority booking, and personal training perks",
            DurationMonths = 1,
            Price = 79.99m,
            MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.MembershipPlans.AddRange(basic, premium, elite);
        db.SaveChanges();

        // Members
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@example.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@example.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Williams", Email = "james.williams@example.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Thompson", Email = "emma.thompson@example.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "David Thompson", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Liam", LastName = "Patel", Email = "liam.patel@example.com", Phone = "555-0601", DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Olivia", LastName = "Kim", Email = "olivia.kim@example.com", Phone = "555-0701", DateOfBirth = new DateOnly(1991, 4, 18), EmergencyContactName = "Daniel Kim", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Noah", LastName = "Garcia", Email = "noah.garcia@example.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 12, 5), EmergencyContactName = "Maria Garcia", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now }
        };

        db.Members.AddRange(members);
        db.SaveChanges();

        // Memberships
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(-15).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(-10).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-20), EndDate = today.AddDays(-20).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(-5).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-8), EndDate = today.AddDays(-8).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-25), EndDate = today.AddDays(-25).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7
            new() { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 8
            new() { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now }
        };

        db.Memberships.AddRange(memberships);
        db.SaveChanges();

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Sarah", LastName = "Martinez", Email = "sarah.martinez@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience. Specializes in Vinyasa and Hatha yoga.", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Mike", LastName = "Johnson", Email = "mike.johnson@zenithfitness.com", Phone = "555-1002", Bio = "Former professional athlete turned fitness instructor. Expert in high-intensity training.", Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@zenithfitness.com", Phone = "555-1003", Bio = "Pilates and meditation specialist. Focused on mind-body connection.", Specializations = "Pilates, Meditation, Yoga", HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Lee", Email = "david.lee@zenithfitness.com", Phone = "555-1004", Bio = "Spin and HIIT specialist with a passion for endurance training.", Specializations = "Spin, HIIT, Boxing", HireDate = new DateOnly(2022, 9, 1), CreatedAt = now, UpdatedAt = now }
        };

        db.Instructors.AddRange(instructors);
        db.SaveChanges();

        // Class Types
        var yoga = new ClassType { Name = "Yoga", Description = "Vinyasa flow yoga focusing on breath and movement", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now };
        var hiit = new ClassType { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var spin = new ClassType { Name = "Spin", Description = "Indoor cycling class with energizing music", DefaultDurationMinutes = 45, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now };
        var pilates = new ClassType { Name = "Pilates", Description = "Core-strengthening exercises with precision and control", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now };
        var boxing = new ClassType { Name = "Boxing", Description = "Boxing fitness combining technique and cardio", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var meditation = new ClassType { Name = "Meditation", Description = "Guided meditation for stress relief and mindfulness", DefaultDurationMinutes = 30, DefaultCapacity = 30, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now };

        db.ClassTypes.AddRange(yoga, hiit, spin, pilates, boxing, meditation);
        db.SaveChanges();

        // Class Schedules - spread over the next 7 days
        var tomorrow = now.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Day 1
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 2, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = spin.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddHours(17), EndTime = tomorrow.AddHours(17).AddMinutes(45), Capacity = 25, CurrentEnrollment = 1, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day 2
            new() { ClassTypeId = pilates.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(9), Capacity = 15, CurrentEnrollment = 2, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11), Capacity = 12, CurrentEnrollment = 1, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 3
            new() { ClassTypeId = meditation.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(7).AddMinutes(30), Capacity = 30, CurrentEnrollment = 2, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(2).AddHours(18), EndTime = tomorrow.AddDays(2).AddHours(19), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            // Day 4 - FULL class with waitlist
            new() { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(3).AddHours(9), EndTime = tomorrow.AddDays(3).AddHours(9).AddMinutes(45), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 5
            new() { ClassTypeId = spin.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(4).AddHours(6), EndTime = tomorrow.AddDays(4).AddHours(6).AddMinutes(45), Capacity = 25, CurrentEnrollment = 2, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = pilates.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(4).AddHours(10), EndTime = tomorrow.AddDays(4).AddHours(11), Capacity = 15, CurrentEnrollment = 1, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            // Day 6
            new() { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(5).AddHours(16), EndTime = tomorrow.AddDays(5).AddHours(17), Capacity = 12, CurrentEnrollment = 2, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(5).AddHours(8), EndTime = tomorrow.AddDays(5).AddHours(9), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now }
        };

        db.ClassSchedules.AddRange(schedules);
        db.SaveChanges();

        // Bookings - create confirmed, waitlisted, attended, cancelled, no-show
        var bookings = new List<Booking>
        {
            // Class 1 (Yoga tomorrow) - 3 confirmed
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 2 (HIIT tomorrow) - 2 confirmed
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },

            // Class 3 (Spin tomorrow) - 1 confirmed
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },

            // Class 4 (Pilates Day 2) - 2 confirmed (premium)
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 5 (Boxing Day 2) - 1 confirmed (premium)
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 6 (Meditation Day 3) - 2 confirmed
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 8 (FULL HIIT Day 4) - 3 confirmed + 2 waitlisted
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[5].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },

            // A cancelled booking
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[2].Id, Status = BookingStatus.Cancelled, BookingDate = now.AddDays(-1), CancellationDate = now, CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // Class 7 (Yoga Day 3) - 3 confirmed
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now }
        };

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }
}
