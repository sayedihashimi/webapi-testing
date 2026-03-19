using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (db.MembershipPlans.Any()) return;

        // Membership Plans
        var basic = new MembershipPlan { Id = 1, Name = "Basic", Description = "Access to standard classes with limited weekly bookings.", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false };
        var premium = new MembershipPlan { Id = 2, Name = "Premium", Description = "Full access including premium classes with higher weekly booking limits.", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true };
        var elite = new MembershipPlan { Id = 3, Name = "Elite", Description = "Unlimited access to all classes including premium. The ultimate fitness experience.", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true };
        db.MembershipPlans.AddRange(basic, premium, elite);

        // Members
        var members = new List<Member>
        {
            new() { Id = 1, FirstName = "Emma", LastName = "Johnson", Email = "emma.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Robert Johnson", EmergencyContactPhone = "555-0102", JoinDate = new DateOnly(2024, 1, 10) },
            new() { Id = 2, FirstName = "Liam", LastName = "Williams", Email = "liam.williams@email.com", Phone = "555-0103", DateOfBirth = new DateOnly(1988, 7, 22), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0104", JoinDate = new DateOnly(2024, 2, 5) },
            new() { Id = 3, FirstName = "Sophia", LastName = "Brown", Email = "sophia.brown@email.com", Phone = "555-0105", DateOfBirth = new DateOnly(1995, 11, 8), EmergencyContactName = "Michael Brown", EmergencyContactPhone = "555-0106", JoinDate = new DateOnly(2024, 3, 1) },
            new() { Id = 4, FirstName = "Noah", LastName = "Davis", Email = "noah.davis@email.com", Phone = "555-0107", DateOfBirth = new DateOnly(1992, 5, 30), EmergencyContactName = "Jessica Davis", EmergencyContactPhone = "555-0108", JoinDate = new DateOnly(2024, 1, 20) },
            new() { Id = 5, FirstName = "Olivia", LastName = "Martinez", Email = "olivia.martinez@email.com", Phone = "555-0109", DateOfBirth = new DateOnly(1998, 9, 12), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0110", JoinDate = new DateOnly(2024, 4, 15) },
            new() { Id = 6, FirstName = "James", LastName = "Garcia", Email = "james.garcia@email.com", Phone = "555-0111", DateOfBirth = new DateOnly(1985, 1, 25), EmergencyContactName = "Maria Garcia", EmergencyContactPhone = "555-0112", JoinDate = new DateOnly(2024, 5, 1) },
            new() { Id = 7, FirstName = "Ava", LastName = "Wilson", Email = "ava.wilson@email.com", Phone = "555-0113", DateOfBirth = new DateOnly(2000, 4, 18), EmergencyContactName = "Thomas Wilson", EmergencyContactPhone = "555-0114", JoinDate = new DateOnly(2024, 6, 10) },
            new() { Id = 8, FirstName = "Mason", LastName = "Taylor", Email = "mason.taylor@email.com", Phone = "555-0115", DateOfBirth = new DateOnly(1993, 12, 3), EmergencyContactName = "Linda Taylor", EmergencyContactPhone = "555-0116", JoinDate = new DateOnly(2024, 7, 1) },
        };
        db.Members.AddRange(members);

        // Memberships
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var memberships = new List<Membership>
        {
            new() { Id = 1, MemberId = 1, MembershipPlanId = 2, StartDate = today.AddMonths(-1).AddDays(15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 2, MemberId = 2, MembershipPlanId = 3, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = today.AddDays(-25), EndDate = today.AddDays(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            // Expired membership for member 7
            new() { Id = 7, MemberId = 7, MembershipPlanId = 1, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            // Cancelled membership for member 8
            new() { Id = 8, MemberId = 8, MembershipPlanId = 2, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded },
        };
        db.Memberships.AddRange(memberships);

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@zenithfitness.com", Phone = "555-0201", Bio = "Certified yoga instructor with 10 years of experience. Specializes in Vinyasa and Hatha yoga.", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15) },
            new() { Id = 2, FirstName = "Marcus", LastName = "Thompson", Email = "marcus.thompson@zenithfitness.com", Phone = "555-0202", Bio = "Former professional athlete turned fitness coach. Expert in high-intensity training.", Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2021, 3, 1) },
            new() { Id = 3, FirstName = "Elena", LastName = "Rodriguez", Email = "elena.rodriguez@zenithfitness.com", Phone = "555-0203", Bio = "Pilates and dance instructor with a background in physical therapy.", Specializations = "Pilates, Yoga, Meditation", HireDate = new DateOnly(2022, 6, 10) },
            new() { Id = 4, FirstName = "David", LastName = "Kim", Email = "david.kim@zenithfitness.com", Phone = "555-0204", Bio = "CrossFit Level 3 trainer and spinning enthusiast. Loves pushing limits.", Specializations = "Spin, HIIT, Boxing", HireDate = new DateOnly(2023, 1, 5) },
        };
        db.Instructors.AddRange(instructors);

        // Class Types
        var classTypes = new List<ClassType>
        {
            new() { Id = 1, Name = "Yoga", Description = "Mind-body practice combining physical postures, breathing techniques, and meditation.", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels },
            new() { Id = 2, Name = "HIIT", Description = "High-Intensity Interval Training for maximum calorie burn and cardiovascular fitness.", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate },
            new() { Id = 3, Name = "Spin", Description = "Indoor cycling class set to motivating music with varying intensity.", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.AllLevels },
            new() { Id = 4, Name = "Pilates", Description = "Core-focused strength training emphasizing controlled movements and flexibility.", DefaultDurationMinutes = 55, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Intermediate },
            new() { Id = 5, Name = "Boxing", Description = "High-energy boxing workout combining cardio, strength, and agility.", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced },
            new() { Id = 6, Name = "Meditation", Description = "Guided meditation sessions for stress relief and mental clarity.", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner },
        };
        db.ClassTypes.AddRange(classTypes);

        // Class Schedules - spread over next 7 days
        var now = DateTime.UtcNow;
        var baseDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var schedules = new List<ClassSchedule>
        {
            // Today
            new() { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(8), Capacity = 20, CurrentEnrollment = 5, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            // Tomorrow
            new() { Id = 3, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddDays(2).AddHours(6), EndTime = baseDate.AddDays(2).AddHours(6).AddMinutes(45), Capacity = 20, CurrentEnrollment = 12, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 4, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(2).AddHours(10), EndTime = baseDate.AddDays(2).AddHours(10).AddMinutes(55), Capacity = 15, CurrentEnrollment = 8, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 3
            new() { Id = 5, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(3).AddHours(17), EndTime = baseDate.AddDays(3).AddHours(18), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 1, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 6, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(3).AddHours(19), EndTime = baseDate.AddDays(3).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 4
            new() { Id = 7, ClassTypeId = 1, InstructorId = 3, StartTime = baseDate.AddDays(4).AddHours(8), EndTime = baseDate.AddDays(4).AddHours(9), Capacity = 20, CurrentEnrollment = 10, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 8, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(4).AddHours(12), EndTime = baseDate.AddDays(4).AddHours(12).AddMinutes(45), Capacity = 15, CurrentEnrollment = 7, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            // Day 5
            new() { Id = 9, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddDays(5).AddHours(7), EndTime = baseDate.AddDays(5).AddHours(7).AddMinutes(45), Capacity = 20, CurrentEnrollment = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 10, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(5).AddHours(18), EndTime = baseDate.AddDays(5).AddHours(19), Capacity = 12, CurrentEnrollment = 5, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            // Day 6
            new() { Id = 11, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(6).AddHours(9), EndTime = baseDate.AddDays(6).AddHours(9).AddMinutes(55), Capacity = 15, CurrentEnrollment = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Cancelled class
            new() { Id = 12, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(6).AddHours(18), EndTime = baseDate.AddDays(6).AddHours(18).AddMinutes(30), Capacity = 25, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" },
        };
        db.ClassSchedules.AddRange(schedules);

        // Bookings
        var bookings = new List<Booking>
        {
            // Class 1 (Yoga, tomorrow 7am) - 5 confirmed
            new() { Id = 1, ClassScheduleId = 1, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 2, ClassScheduleId = 1, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 3, ClassScheduleId = 1, MemberId = 3, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12) },
            new() { Id = 4, ClassScheduleId = 1, MemberId = 4, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6) },
            new() { Id = 5, ClassScheduleId = 1, MemberId = 5, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3) },

            // Class 2 (HIIT, tomorrow 9am) - full (15) + 2 waitlisted
            new() { Id = 6, ClassScheduleId = 2, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2) },
            new() { Id = 7, ClassScheduleId = 2, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2) },
            new() { Id = 8, ClassScheduleId = 2, MemberId = 4, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-1) },
            new() { Id = 9, ClassScheduleId = 2, MemberId = 6, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddMinutes(-30) },

            // Class 5 (Boxing, day 3) - full + 1 waitlisted
            new() { Id = 10, ClassScheduleId = 5, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 11, ClassScheduleId = 5, MemberId = 6, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-2) },

            // Cancelled booking
            new() { Id = 12, ClassScheduleId = 3, MemberId = 1, Status = BookingStatus.Cancelled, BookingDate = now.AddDays(-3), CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict" },

            // Attended booking (past-looking but on schedule 6)
            new() { Id = 13, ClassScheduleId = 6, MemberId = 3, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5) },

            // No-show
            new() { Id = 14, ClassScheduleId = 7, MemberId = 5, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-10) },

            // More confirmed
            new() { Id = 15, ClassScheduleId = 4, MemberId = 4, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8) },
        };
        db.Bookings.AddRange(bookings);

        await db.SaveChangesAsync();
    }
}
