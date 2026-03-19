using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basic = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes", DurationMonths = 1,
            Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now
        };
        var premium = new MembershipPlan
        {
            Name = "Premium", Description = "Access to standard and premium classes", DurationMonths = 1,
            Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        var elite = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes", DurationMonths = 1,
            Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        db.MembershipPlans.AddRange(basic, premium, elite);
        await db.SaveChangesAsync();

        // Members
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102",
                JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Brian", LastName = "Smith", Email = "brian.smith@email.com", Phone = "555-0201",
                DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Carol Smith", EmergencyContactPhone = "555-0202",
                JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Clara", LastName = "Davis", Email = "clara.davis@email.com", Phone = "555-0301",
                DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Dan Davis", EmergencyContactPhone = "555-0302",
                JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Wilson", Email = "david.wilson@email.com", Phone = "555-0401",
                DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Eva Wilson", EmergencyContactPhone = "555-0402",
                JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Brown", Email = "emma.brown@email.com", Phone = "555-0501",
                DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Frank Brown", EmergencyContactPhone = "555-0502",
                JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Frank", LastName = "Taylor", Email = "frank.taylor@email.com", Phone = "555-0601",
                DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Grace Taylor", EmergencyContactPhone = "555-0602",
                JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Grace", LastName = "Anderson", Email = "grace.anderson@email.com", Phone = "555-0701",
                DateOfBirth = new DateOnly(1991, 4, 18), EmergencyContactName = "Henry Anderson", EmergencyContactPhone = "555-0702",
                JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Henry", LastName = "Martinez", Email = "henry.martinez@email.com", Phone = "555-0801",
                DateOfBirth = new DateOnly(1987, 12, 5), EmergencyContactName = "Iris Martinez", EmergencyContactPhone = "555-0802",
                JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now }
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // Memberships
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-15),
                EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-10),
                EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-5),
                EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-20),
                EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-8),
                EndDate = today.AddDays(22), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-3),
                EndDate = today.AddDays(27), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7 (Grace)
            new() { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-2),
                EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Cancelled membership for member 8 (Henry)
            new() { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-3),
                EndDate = today.AddMonths(-2), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now }
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@zenithstudio.com", Phone = "555-1001",
                Bio = "Certified yoga instructor with 10 years of experience", Specializations = "Yoga, Pilates, Meditation",
                HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Mike", LastName = "Torres", Email = "mike.torres@zenithstudio.com", Phone = "555-1002",
                Bio = "Former professional boxer and HIIT specialist", Specializations = "HIIT, Boxing, Spin",
                HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Lisa", LastName = "Park", Email = "lisa.park@zenithstudio.com", Phone = "555-1003",
                Bio = "Pilates and barre expert with dance background", Specializations = "Pilates, Yoga",
                HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Miller", Email = "james.miller@zenithstudio.com", Phone = "555-1004",
                Bio = "Spinning champion and cardio enthusiast", Specializations = "Spin, HIIT",
                HireDate = new DateOnly(2022, 8, 20), CreatedAt = now, UpdatedAt = now }
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // Class Types
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Relaxing yoga sessions for all levels", DefaultDurationMinutes = 60,
                DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-intensity interval training", DefaultDurationMinutes = 45,
                DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class", DefaultDurationMinutes = 45,
                DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core strengthening and flexibility", DefaultDurationMinutes = 60,
                DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing fitness class with personal attention", DefaultDurationMinutes = 60,
                DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and mindfulness", DefaultDurationMinutes = 30,
                DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now }
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // Class Schedules (next 7 days)
        var tomorrow = now.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Day 1
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddHours(8), EndTime = tomorrow.AddHours(9), Capacity = 20,
                CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddHours(10), EndTime = tomorrow.AddHours(10).AddMinutes(45), Capacity = 15,
                CurrentEnrollment = 2, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddHours(17), EndTime = tomorrow.AddHours(17).AddMinutes(45), Capacity = 20,
                CurrentEnrollment = 1, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 2
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(1).AddHours(9), EndTime = tomorrow.AddDays(1).AddHours(10), Capacity = 15,
                CurrentEnrollment = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(1).AddHours(11), EndTime = tomorrow.AddDays(1).AddHours(12), Capacity = 10,
                CurrentEnrollment = 2, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 3 - full class with waitlist
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(7).AddMinutes(30), Capacity = 3,
                CurrentEnrollment = 3, WaitlistCount = 1, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(2).AddHours(10), EndTime = tomorrow.AddDays(2).AddHours(11), Capacity = 20,
                CurrentEnrollment = 1, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 4
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(3).AddHours(6), EndTime = tomorrow.AddDays(3).AddHours(6).AddMinutes(45), Capacity = 15,
                CurrentEnrollment = 1, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(3).AddHours(18), EndTime = tomorrow.AddDays(3).AddHours(18).AddMinutes(45), Capacity = 20,
                CurrentEnrollment = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 5
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(4).AddHours(9), EndTime = tomorrow.AddDays(4).AddHours(10), Capacity = 15,
                CurrentEnrollment = 1, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 6 - cancelled class
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(5).AddHours(14), EndTime = tomorrow.AddDays(5).AddHours(15), Capacity = 10,
                CurrentEnrollment = 0, Room = "Main Floor", Status = ClassScheduleStatus.Cancelled,
                CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
            // Day 6 - another class
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(5).AddHours(8), EndTime = tomorrow.AddDays(5).AddHours(9), Capacity = 20,
                CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now }
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // Bookings
        var bookings = new List<Booking>
        {
            // Schedule 1 (Yoga tomorrow) - 3 confirmed
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-12),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-10),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-8),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 2 (HIIT tomorrow) - 2 confirmed
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-11),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-9),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 3 (Spin tomorrow) - 1 confirmed
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-7),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 4 (Pilates day 2) - 2 confirmed
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-6),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-5),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 5 (Boxing day 2 - premium) - 2 confirmed
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-4),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-3),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 6 (Meditation day 3 - full with waitlist, capacity 3) - 3 confirmed + 1 waitlisted
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-6),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-5),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-4),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-3),
                Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },

            // Schedule 7 (Yoga day 3) - 1 confirmed
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-2),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Schedule 8 (HIIT day 4) - 1 cancelled
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-1),
                Status = BookingStatus.Cancelled, CancellationDate = now.AddHours(-1), CancellationReason = "Schedule conflict",
                CreatedAt = now, UpdatedAt = now },

            // Schedule 10 (Pilates day 5) - 1 confirmed
            new() { ClassScheduleId = schedules[9].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-1),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
        };
        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
