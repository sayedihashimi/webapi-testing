using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(StudioDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
        {
            return;
        }

        // --- Membership Plans ---
        var basic = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false
        };
        var premium = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true
        };
        var elite = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes and premium facilities",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true
        };
        db.MembershipPlans.AddRange(basic, premium, elite);
        await db.SaveChangesAsync();

        // --- Members ---
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102" },
            new Member { FirstName = "Carlos", LastName = "Martinez", Email = "carlos@example.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Maria Martinez", EmergencyContactPhone = "555-0202" },
            new Member { FirstName = "Diana", LastName = "Chen", Email = "diana@example.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Wei Chen", EmergencyContactPhone = "555-0302" },
            new Member { FirstName = "Erik", LastName = "Andersen", Email = "erik@example.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Lena Andersen", EmergencyContactPhone = "555-0402" },
            new Member { FirstName = "Fatima", LastName = "Al-Hassan", Email = "fatima@example.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 6, 12), EmergencyContactName = "Omar Al-Hassan", EmergencyContactPhone = "555-0502" },
            new Member { FirstName = "George", LastName = "Papadopoulos", Email = "george@example.com", Phone = "555-0601", DateOfBirth = new DateOnly(1991, 9, 25), EmergencyContactName = "Elena Papadopoulos", EmergencyContactPhone = "555-0602" },
            new Member { FirstName = "Hannah", LastName = "Williams", Email = "hannah@example.com", Phone = "555-0701", DateOfBirth = new DateOnly(1987, 4, 3), EmergencyContactName = "Tom Williams", EmergencyContactPhone = "555-0702" },
            new Member { FirstName = "Ivan", LastName = "Petrov", Email = "ivan@example.com", Phone = "555-0801", DateOfBirth = new DateOnly(1993, 12, 18), EmergencyContactName = "Olga Petrov", EmergencyContactPhone = "555-0802" },
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // --- Memberships ---
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var memberships = new[]
        {
            new Membership { MemberId = members[0].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[1].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[4].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-45), EndDate = today.AddDays(-15), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[5].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-7), EndDate = today.AddDays(23), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[6].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-60), EndDate = today.AddDays(-30), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[7].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-3), EndDate = today.AddDays(27), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // --- Instructors ---
        var instructors = new[]
        {
            new Instructor { FirstName = "Sarah", LastName = "Thompson", Email = "sarah@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience.", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2020, 1, 15) },
            new Instructor { FirstName = "Marcus", LastName = "Rivera", Email = "marcus@zenithfitness.com", Phone = "555-1002", Bio = "Former professional boxer, specializing in HIIT and combat fitness.", Specializations = "HIIT,Boxing,Strength", HireDate = new DateOnly(2019, 6, 1) },
            new Instructor { FirstName = "Yuki", LastName = "Tanaka", Email = "yuki@zenithfitness.com", Phone = "555-1003", Bio = "Spin and endurance specialist with cycling competition background.", Specializations = "Spin,HIIT,Endurance", HireDate = new DateOnly(2021, 3, 10) },
            new Instructor { FirstName = "Priya", LastName = "Sharma", Email = "priya@zenithfitness.com", Phone = "555-1004", Bio = "Meditation and mindfulness expert, certified Pilates trainer.", Specializations = "Meditation,Yoga,Pilates", HireDate = new DateOnly(2022, 8, 20) },
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // --- Class Types ---
        var classTypes = new[]
        {
            new ClassType { Name = "Yoga Flow", Description = "A dynamic vinyasa-style yoga class", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels },
            new ClassType { Name = "HIIT Blast", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate },
            new ClassType { Name = "Spin Surge", Description = "Indoor cycling at high intensity", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced },
            new ClassType { Name = "Pilates Core", Description = "Core-focused Pilates for strength and flexibility", DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner },
            new ClassType { Name = "Boxing Fundamentals", Description = "Learn boxing technique and build endurance", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 700, DifficultyLevel = DifficultyLevel.Intermediate },
            new ClassType { Name = "Guided Meditation", Description = "Mindfulness and deep relaxation session", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner },
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // --- Class Schedules (next 7 days) ---
        var now = DateTime.UtcNow;
        var baseDate = now.Date.AddDays(1);

        var schedules = new[]
        {
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddHours(7), EndTime = baseDate.AddHours(8), Capacity = 20, CurrentEnrollment = 5, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Studio B" },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddHours(11), EndTime = baseDate.AddHours(11).AddMinutes(45), Capacity = 12, CurrentEnrollment = 10, Room = "Spin Room" },
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(1).AddHours(8), EndTime = baseDate.AddDays(1).AddHours(8).AddMinutes(50), Capacity = 15, CurrentEnrollment = 8, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(1).AddHours(10), EndTime = baseDate.AddDays(1).AddHours(11), Capacity = 10, CurrentEnrollment = 10, WaitlistCount = 1, Room = "Boxing Ring" },
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(1).AddHours(17), EndTime = baseDate.AddDays(1).AddHours(17).AddMinutes(30), Capacity = 25, CurrentEnrollment = 3, Room = "Zen Room" },
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(2).AddHours(7), EndTime = baseDate.AddDays(2).AddHours(8), Capacity = 20, CurrentEnrollment = 12, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(2).AddHours(18), EndTime = baseDate.AddDays(2).AddHours(18).AddMinutes(45), Capacity = 15, CurrentEnrollment = 7, Room = "Studio B" },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(3).AddHours(6), EndTime = baseDate.AddDays(3).AddHours(6).AddMinutes(45), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 3, Room = "Spin Room" },
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(4).AddHours(9), EndTime = baseDate.AddDays(4).AddHours(9).AddMinutes(50), Capacity = 15, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" },
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(5).AddHours(19), EndTime = baseDate.AddDays(5).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 2, Room = "Zen Room" },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(6).AddHours(10), EndTime = baseDate.AddDays(6).AddHours(11), Capacity = 10, CurrentEnrollment = 4, Room = "Boxing Ring" },
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // --- Bookings ---
        var bookings = new[]
        {
            // Yoga class - confirmed
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed },

            // HIIT class (full) - confirmed + waitlisted
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[5].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2 },

            // Spin class
            new Booking { ClassScheduleId = schedules[2].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed },

            // Pilates - cancelled booking
            new Booking { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, Status = BookingStatus.Cancelled, CancellationDate = DateTime.UtcNow.AddDays(-1), CancellationReason = "Schedule conflict" },
            new Booking { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed },

            // Boxing (full) - with waitlist
            new Booking { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },

            // Past-like bookings with attended/no-show
            new Booking { ClassScheduleId = schedules[6].Id, MemberId = members[7].Id, Status = BookingStatus.Confirmed },
            new Booking { ClassScheduleId = schedules[7].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed },
        };
        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
