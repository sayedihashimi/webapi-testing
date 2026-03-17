using FitnessStudioApi.Data;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return;

        // ── Membership Plans ──
        var basic = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes and basic facilities",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false
        };
        var premium = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all standard classes and premium facilities",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true
        };
        var elite = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes and facilities including personal training",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true
        };
        db.MembershipPlans.AddRange(basic, premium, elite);
        await db.SaveChangesAsync();

        // ── Members ──
        var members = new List<Member>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101",
                     DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Mike Johnson", EmergencyContactPhone = "555-0102" },
            new() { FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0103",
                     DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Williams", EmergencyContactPhone = "555-0104" },
            new() { FirstName = "Emily", LastName = "Brown", Email = "emily.brown@email.com", Phone = "555-0105",
                     DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Tom Brown", EmergencyContactPhone = "555-0106" },
            new() { FirstName = "Michael", LastName = "Davis", Email = "michael.davis@email.com", Phone = "555-0107",
                     DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Karen Davis", EmergencyContactPhone = "555-0108" },
            new() { FirstName = "Jessica", LastName = "Martinez", Email = "jessica.martinez@email.com", Phone = "555-0109",
                     DateOfBirth = new DateOnly(1995, 5, 17), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0110" },
            new() { FirstName = "David", LastName = "Anderson", Email = "david.anderson@email.com", Phone = "555-0111",
                     DateOfBirth = new DateOnly(1993, 9, 3), EmergencyContactName = "Rachel Anderson", EmergencyContactPhone = "555-0112" },
            new() { FirstName = "Amanda", LastName = "Taylor", Email = "amanda.taylor@email.com", Phone = "555-0113",
                     DateOfBirth = new DateOnly(1991, 12, 25), EmergencyContactName = "Robert Taylor", EmergencyContactPhone = "555-0114" },
            new() { FirstName = "Chris", LastName = "Thomas", Email = "chris.thomas@email.com", Phone = "555-0115",
                     DateOfBirth = new DateOnly(1987, 6, 11), EmergencyContactName = "Nancy Thomas", EmergencyContactPhone = "555-0116" }
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // ── Memberships ──
        var today = DateOnly.FromDateTime(DateTime.Today);
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[1].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[2].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[4].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-8), EndDate = today.AddDays(22), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[5].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-3), EndDate = today.AddDays(27), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            // Expired memberships
            new() { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new() { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid }
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // ── Instructors ──
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Alex", LastName = "Rivera", Email = "alex.rivera@zenith.com", Phone = "555-0201",
                     Bio = "Certified yoga instructor with 10 years of experience", Specializations = "Yoga,Pilates,Meditation",
                     HireDate = new DateOnly(2020, 1, 15) },
            new() { FirstName = "Morgan", LastName = "Chen", Email = "morgan.chen@zenith.com", Phone = "555-0202",
                     Bio = "Former competitive cyclist and HIIT specialist", Specializations = "HIIT,Spin,Boxing",
                     HireDate = new DateOnly(2019, 6, 1) },
            new() { FirstName = "Taylor", LastName = "Nguyen", Email = "taylor.nguyen@zenith.com", Phone = "555-0203",
                     Bio = "Professional boxer turned fitness instructor", Specializations = "Boxing,HIIT",
                     HireDate = new DateOnly(2021, 3, 10) },
            new() { FirstName = "Jordan", LastName = "Patel", Email = "jordan.patel@zenith.com", Phone = "555-0204",
                     Bio = "Mindfulness expert and meditation practitioner", Specializations = "Meditation,Yoga,Pilates",
                     HireDate = new DateOnly(2022, 8, 20) }
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // ── Class Types ──
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Traditional yoga combining flexibility, strength, and mindfulness",
                    DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250,
                    DifficultyLevel = DifficultyLevel.AllLevels },
            new() { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn",
                    DefaultDurationMinutes = 45, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 500,
                    DifficultyLevel = DifficultyLevel.Advanced },
            new() { Name = "Spin", Description = "Indoor cycling class with music-driven intervals",
                    DefaultDurationMinutes = 45, DefaultCapacity = 30, IsPremium = true, CaloriesPerSession = 450,
                    DifficultyLevel = DifficultyLevel.Intermediate },
            new() { Name = "Pilates", Description = "Core-strengthening exercises focusing on alignment and control",
                    DefaultDurationMinutes = 55, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300,
                    DifficultyLevel = DifficultyLevel.Beginner },
            new() { Name = "Boxing", Description = "Boxing-inspired fitness class combining cardio and strength",
                    DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = true, CaloriesPerSession = 600,
                    DifficultyLevel = DifficultyLevel.Advanced },
            new() { Name = "Meditation", Description = "Guided meditation session for stress relief and mental clarity",
                    DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50,
                    DifficultyLevel = DifficultyLevel.Beginner }
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // ── Class Schedules ── (12 over next 7 days)
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Day 1
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                    StartTime = baseDate.AddHours(7), EndTime = baseDate.AddHours(8),
                    Capacity = 20, CurrentEnrollment = 5, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                    StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(9).AddMinutes(45),
                    Capacity = 25, CurrentEnrollment = 10, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            // Day 2
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[1].Id,
                    StartTime = baseDate.AddDays(1).AddHours(8), EndTime = baseDate.AddDays(1).AddHours(8).AddMinutes(45),
                    Capacity = 30, CurrentEnrollment = 28, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled },
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id,
                    StartTime = baseDate.AddDays(1).AddHours(10), EndTime = baseDate.AddDays(1).AddHours(10).AddMinutes(55),
                    Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 3
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[2].Id,
                    StartTime = baseDate.AddDays(2).AddHours(11), EndTime = baseDate.AddDays(2).AddHours(12),
                    Capacity = 20, CurrentEnrollment = 8, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[3].Id,
                    StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(17).AddMinutes(30),
                    Capacity = 25, CurrentEnrollment = 3, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled },
            // Day 4
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id,
                    StartTime = baseDate.AddDays(3).AddHours(7), EndTime = baseDate.AddDays(3).AddHours(8),
                    Capacity = 20, CurrentEnrollment = 12, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[2].Id,
                    StartTime = baseDate.AddDays(3).AddHours(18), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(45),
                    Capacity = 25, CurrentEnrollment = 20, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            // Day 5
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[1].Id,
                    StartTime = baseDate.AddDays(4).AddHours(6), EndTime = baseDate.AddDays(4).AddHours(6).AddMinutes(45),
                    Capacity = 30, CurrentEnrollment = 15, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled },
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id,
                    StartTime = baseDate.AddDays(4).AddHours(12), EndTime = baseDate.AddDays(4).AddHours(12).AddMinutes(55),
                    Capacity = 15, CurrentEnrollment = 7, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 6 - Cancelled class
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[2].Id,
                    StartTime = baseDate.AddDays(5).AddHours(10), EndTime = baseDate.AddDays(5).AddHours(11),
                    Capacity = 20, CurrentEnrollment = 0, Room = "Studio B",
                    Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" },
            // Day 6 - Another class
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[3].Id,
                    StartTime = baseDate.AddDays(5).AddHours(19), EndTime = baseDate.AddDays(5).AddHours(19).AddMinutes(30),
                    Capacity = 25, CurrentEnrollment = 2, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled }
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // ── Bookings ── (15 in various states)
        var bookings = new List<Booking>
        {
            // Confirmed bookings for class 1 (Yoga tomorrow)
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed },

            // Confirmed bookings for class 2 (HIIT tomorrow)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed },

            // Full class with waitlist (Pilates day 2) - schedules[3] is at capacity
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[5].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2 },

            // Boxing class bookings
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed },

            // Cancelled booking
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[5].Id, Status = BookingStatus.Cancelled,
                     CancellationDate = DateTime.UtcNow.AddHours(-2), CancellationReason = "Schedule conflict" },

            // Attended (past-like, but for seed data we set status directly)
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },

            // No-show
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed },

            // Meditation booking
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed }
        };
        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
