using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // ========== Membership Plans ==========
        var basic = new MembershipPlan { Name = "Basic", Description = "Access to standard classes with limited weekly bookings.", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now };
        var premium = new MembershipPlan { Name = "Premium", Description = "Access to all classes including premium. More weekly bookings.", DurationMonths = 3, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        var elite = new MembershipPlan { Name = "Elite", Description = "Unlimited access to all classes and premium features.", DurationMonths = 6, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        db.MembershipPlans.AddRange(basic, premium, elite);
        await db.SaveChangesAsync();

        // ========== Members ==========
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Brian", LastName = "Smith", Email = "brian.smith@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Carol Smith", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Carla", LastName = "Martinez", Email = "carla.martinez@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "David Martinez", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Derek", LastName = "Williams", Email = "derek.williams@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Emily Williams", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Elena", LastName = "Garcia", Email = "elena.garcia@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 6, 12), EmergencyContactName = "Frank Garcia", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Felix", LastName = "Brown", Email = "felix.brown@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Grace Brown", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Gina", LastName = "Davis", Email = "gina.davis@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1991, 4, 18), EmergencyContactName = "Henry Davis", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Hector", LastName = "Wilson", Email = "hector.wilson@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 12, 5), EmergencyContactName = "Irene Wilson", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now }
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // ========== Memberships ==========
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-10), EndDate = today.AddMonths(6).AddDays(-10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for Gina
            new() { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Cancelled membership for Hector
            new() { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-4), EndDate = today.AddMonths(-1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now }
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // ========== Instructors ==========
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@zenith.com", Phone = "555-1001", Bio = "Certified yoga and meditation instructor with 10 years of experience.", Specializations = "Yoga, Meditation, Pilates", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Taylor", Email = "marcus.taylor@zenith.com", Phone = "555-1002", Bio = "Former professional boxer turned fitness instructor.", Specializations = "Boxing, HIIT, Strength Training", HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Lisa", LastName = "Nguyen", Email = "lisa.nguyen@zenith.com", Phone = "555-1003", Bio = "Spin and cardio specialist passionate about high-energy workouts.", Specializations = "Spin, HIIT, Dance Fitness", HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Patel", Email = "james.patel@zenith.com", Phone = "555-1004", Bio = "Pilates and flexibility expert with rehabilitation background.", Specializations = "Pilates, Yoga, Stretching", HireDate = new DateOnly(2022, 9, 1), CreatedAt = now, UpdatedAt = now }
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // ========== Class Types ==========
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Relaxing yoga flow for all levels.", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-intensity interval training to burn maximum calories.", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class with energizing music.", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core strengthening and flexibility improvement.", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing class with personal attention from instructors.", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and mindfulness session.", DefaultDurationMinutes = 30, DefaultCapacity = 8, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now }
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // ========== Class Schedules (next 7 days) ==========
        var tomorrow = now.Date.AddDays(1);

        var schedules = new List<ClassSchedule>
        {
            // Day 1 – tomorrow
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(8), EndTime = tomorrow.AddHours(9), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(10), EndTime = tomorrow.AddHours(10).AddMinutes(45), Capacity = 15, CurrentEnrollment = 5, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 2
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(1).AddHours(7), EndTime = tomorrow.AddDays(1).AddHours(7).AddMinutes(45), Capacity = 12, CurrentEnrollment = 4, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(1).AddHours(9), EndTime = tomorrow.AddDays(1).AddHours(10), Capacity = 15, CurrentEnrollment = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 3 – full class (capacity reached) and one with waitlist
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(2).AddHours(11), EndTime = tomorrow.AddDays(2).AddHours(12), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(2).AddHours(14), EndTime = tomorrow.AddDays(2).AddHours(14).AddMinutes(30), Capacity = 8, CurrentEnrollment = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 4
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(3).AddHours(8), EndTime = tomorrow.AddDays(3).AddHours(9), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(3).AddHours(17), EndTime = tomorrow.AddDays(3).AddHours(17).AddMinutes(45), Capacity = 15, CurrentEnrollment = 6, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 5
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(4).AddHours(6).AddMinutes(30), EndTime = tomorrow.AddDays(4).AddHours(7).AddMinutes(15), Capacity = 12, CurrentEnrollment = 1, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 6
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(5).AddHours(9), EndTime = tomorrow.AddDays(5).AddHours(10), Capacity = 15, CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(5).AddHours(12), EndTime = tomorrow.AddDays(5).AddHours(13), Capacity = 10, CurrentEnrollment = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(6).AddHours(8), EndTime = tomorrow.AddDays(6).AddHours(9), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now }
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // ========== Bookings ==========
        var bookings = new List<Booking>
        {
            // Class 1 (tomorrow yoga) – 3 confirmed
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 2 (tomorrow HIIT) – 5 confirmed
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 3 (day 2 spin) – 4 confirmed
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Class 5 (day 3 boxing, premium, FULL cap=3) – 3 confirmed + 2 waitlisted
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id is var _ ? members[0].Id : 0, Status = BookingStatus.Cancelled, CancellationDate = now.AddHours(-1), CancellationReason = "Schedule conflict", BookingDate = now.AddHours(-6), CreatedAt = now, UpdatedAt = now },

            // Some cancelled and no-show bookings
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
        };

        // Fix: use a different member for the second waitlist slot
        bookings.Add(new Booking { ClassScheduleId = schedules[4].Id, MemberId = members[5].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now });

        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
