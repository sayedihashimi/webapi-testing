using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class SeedData
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
        {
            return;
        }

        // Membership Plans
        var basicPlan = new MembershipPlan
        {
            Name = "Basic",
            Description = "Access to standard classes with limited weekly bookings",
            DurationMonths = 1,
            Price = 29.99m,
            MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false
        };

        var premiumPlan = new MembershipPlan
        {
            Name = "Premium",
            Description = "Access to all classes including premium with generous weekly bookings",
            DurationMonths = 3,
            Price = 49.99m,
            MaxClassBookingsPerWeek = 7,
            AllowsPremiumClasses = true
        };

        var elitePlan = new MembershipPlan
        {
            Name = "Elite",
            Description = "Unlimited access to all classes and premium features",
            DurationMonths = 12,
            Price = 79.99m,
            MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true
        };

        db.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);

        // Members
        var members = new[]
        {
            new Member
            {
                FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com",
                Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15),
                EmergencyContactName = "Mike Johnson", EmergencyContactPhone = "555-0102"
            },
            new Member
            {
                FirstName = "James", LastName = "Chen", Email = "james.chen@email.com",
                Phone = "555-0103", DateOfBirth = new DateOnly(1985, 7, 22),
                EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0104"
            },
            new Member
            {
                FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com",
                Phone = "555-0105", DateOfBirth = new DateOnly(1992, 11, 8),
                EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0106"
            },
            new Member
            {
                FirstName = "Michael", LastName = "Thompson", Email = "michael.thompson@email.com",
                Phone = "555-0107", DateOfBirth = new DateOnly(1988, 1, 30),
                EmergencyContactName = "Karen Thompson", EmergencyContactPhone = "555-0108"
            },
            new Member
            {
                FirstName = "Olivia", LastName = "Williams", Email = "olivia.williams@email.com",
                Phone = "555-0109", DateOfBirth = new DateOnly(1995, 5, 12),
                EmergencyContactName = "David Williams", EmergencyContactPhone = "555-0110"
            },
            new Member
            {
                FirstName = "Daniel", LastName = "Kim", Email = "daniel.kim@email.com",
                Phone = "555-0111", DateOfBirth = new DateOnly(1993, 9, 25),
                EmergencyContactName = "Amy Kim", EmergencyContactPhone = "555-0112"
            },
            new Member
            {
                FirstName = "Sophia", LastName = "Patel", Email = "sophia.patel@email.com",
                Phone = "555-0113", DateOfBirth = new DateOnly(1991, 2, 18),
                EmergencyContactName = "Raj Patel", EmergencyContactPhone = "555-0114"
            },
            new Member
            {
                FirstName = "Alexander", LastName = "Brown", Email = "alexander.brown@email.com",
                Phone = "555-0115", DateOfBirth = new DateOnly(1987, 6, 5),
                EmergencyContactName = "Jennifer Brown", EmergencyContactPhone = "555-0116"
            }
        };

        db.Members.AddRange(members);

        // Instructors
        var instructors = new[]
        {
            new Instructor
            {
                FirstName = "Maya", LastName = "Roberts", Email = "maya.roberts@zenithfitness.com",
                Phone = "555-0201", Bio = "Certified yoga instructor with 10 years experience",
                Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15)
            },
            new Instructor
            {
                FirstName = "Jake", LastName = "Martinez", Email = "jake.martinez@zenithfitness.com",
                Phone = "555-0202", Bio = "HIIT and boxing specialist, former amateur boxer",
                Specializations = "HIIT, Boxing, Strength Training", HireDate = new DateOnly(2019, 6, 1)
            },
            new Instructor
            {
                FirstName = "Priya", LastName = "Sharma", Email = "priya.sharma@zenithfitness.com",
                Phone = "555-0203", Bio = "Spin and cardio expert with nutrition certification",
                Specializations = "Spin, Cardio, Nutrition", HireDate = new DateOnly(2021, 3, 10)
            },
            new Instructor
            {
                FirstName = "Chris", LastName = "Taylor", Email = "chris.taylor@zenithfitness.com",
                Phone = "555-0204", Bio = "Pilates and rehabilitation specialist",
                Specializations = "Pilates, Rehabilitation, Flexibility", HireDate = new DateOnly(2022, 8, 20)
            }
        };

        db.Instructors.AddRange(instructors);

        // Class Types
        var yoga = new ClassType
        {
            Name = "Yoga Flow", Description = "A dynamic yoga class combining breath with movement",
            DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false,
            CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels
        };

        var hiit = new ClassType
        {
            Name = "HIIT Blast", Description = "High-intensity interval training for maximum calorie burn",
            DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = true,
            CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced
        };

        var spin = new ClassType
        {
            Name = "Spin Cycle", Description = "Indoor cycling class with hill climbs and sprints",
            DefaultDurationMinutes = 45, DefaultCapacity = 25, IsPremium = false,
            CaloriesPerSession = 400, DifficultyLevel = DifficultyLevel.Intermediate
        };

        var pilates = new ClassType
        {
            Name = "Core Pilates", Description = "Strengthen your core with precise Pilates movements",
            DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false,
            CaloriesPerSession = 200, DifficultyLevel = DifficultyLevel.Beginner
        };

        var boxing = new ClassType
        {
            Name = "Boxing Fit", Description = "Non-contact boxing fitness combining technique with cardio",
            DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true,
            CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced
        };

        var meditation = new ClassType
        {
            Name = "Mindful Meditation", Description = "Guided meditation for stress relief and mental clarity",
            DefaultDurationMinutes = 30, DefaultCapacity = 30, IsPremium = false,
            CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner
        };

        db.ClassTypes.AddRange(yoga, hiit, spin, pilates, boxing, meditation);

        await db.SaveChangesAsync();

        // Memberships
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var memberships = new[]
        {
            new Membership { MemberId = members[0].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[1].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddDays(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-4), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[4].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(11), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[6].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[3].Id, MembershipPlanId = basicPlan.Id, StartDate = today, EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Pending }
        };

        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // Class Schedules over next 7 days
        var now = DateTime.UtcNow;
        var baseDate = now.Date;

        var schedules = new[]
        {
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(8), Capacity = 20, CurrentEnrollment = 5, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Studio B" },
            new ClassSchedule { ClassTypeId = spin.Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(1).AddHours(12), EndTime = baseDate.AddDays(1).AddHours(12).AddMinutes(45), Capacity = 25, CurrentEnrollment = 10, Room = "Spin Room" },
            new ClassSchedule { ClassTypeId = pilates.Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(2).AddHours(8), EndTime = baseDate.AddDays(2).AddHours(8).AddMinutes(50), Capacity = 15, CurrentEnrollment = 8, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(18), Capacity = 12, CurrentEnrollment = 6, Room = "Boxing Ring" },
            new ClassSchedule { ClassTypeId = meditation.Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(2).AddHours(19), EndTime = baseDate.AddDays(2).AddHours(19).AddMinutes(30), Capacity = 30, CurrentEnrollment = 3, Room = "Zen Room" },
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(3).AddHours(7), EndTime = baseDate.AddDays(3).AddHours(8), Capacity = 20, CurrentEnrollment = 12, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = spin.Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(3).AddHours(10), EndTime = baseDate.AddDays(3).AddHours(10).AddMinutes(45), Capacity = 25, CurrentEnrollment = 20, Room = "Spin Room" },
            new ClassSchedule { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(4).AddHours(6), EndTime = baseDate.AddDays(4).AddHours(6).AddMinutes(45), Capacity = 15, CurrentEnrollment = 14, Room = "Studio B" },
            new ClassSchedule { ClassTypeId = pilates.Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(4).AddHours(11), EndTime = baseDate.AddDays(4).AddHours(11).AddMinutes(50), Capacity = 15, CurrentEnrollment = 7, Room = "Studio A" },
            new ClassSchedule { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(5).AddHours(17), EndTime = baseDate.AddDays(5).AddHours(18), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 1, Room = "Boxing Ring" },
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(-1).AddHours(7), EndTime = baseDate.AddDays(-1).AddHours(8), Capacity = 20, CurrentEnrollment = 18, Room = "Studio A", Status = ClassScheduleStatus.Completed },
            new ClassSchedule { ClassTypeId = meditation.Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(5).AddHours(19), EndTime = baseDate.AddDays(5).AddHours(19).AddMinutes(30), Capacity = 30, CurrentEnrollment = 0, Room = "Zen Room", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" }
        };

        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // Bookings
        var bookings = new List<Booking>
        {
            // Confirmed bookings
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[6].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed },
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed },

            // Waitlisted bookings (for full classes)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2 },
            new() { ClassScheduleId = schedules[10].Id, MemberId = members[6].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },

            // Attended bookings (past class)
            new() { ClassScheduleId = schedules[11].Id, MemberId = members[0].Id, Status = BookingStatus.Attended, CheckInTime = baseDate.AddDays(-1).AddHours(6).AddMinutes(50) },
            new() { ClassScheduleId = schedules[11].Id, MemberId = members[1].Id, Status = BookingStatus.Attended, CheckInTime = baseDate.AddDays(-1).AddHours(6).AddMinutes(55) },

            // Cancelled booking
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, Status = BookingStatus.Cancelled, CancellationDate = DateTime.UtcNow.AddDays(-1), CancellationReason = "Schedule conflict" },

            // No-show
            new() { ClassScheduleId = schedules[11].Id, MemberId = members[5].Id, Status = BookingStatus.NoShow }
        };

        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
