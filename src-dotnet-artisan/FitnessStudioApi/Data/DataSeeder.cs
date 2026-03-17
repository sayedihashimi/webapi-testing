using FitnessStudioApi.Models;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static void Seed(FitnessDbContext db)
    {
        if (db.MembershipPlans.Any()) return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // ── Membership Plans ──
        var basic = new MembershipPlan { Name = "Basic", Description = "Access to standard classes with limited bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false };
        var premium = new MembershipPlan { Name = "Premium", Description = "Extended access with premium classes and more bookings", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true };
        var elite = new MembershipPlan { Name = "Elite", Description = "Unlimited access to all classes including premium", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true };
        db.MembershipPlans.AddRange(basic, premium, elite);
        db.SaveChanges();

        // ── Members ──
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6) },
            new Member { FirstName = "Brian", LastName = "Smith", Email = "brian@example.com", Phone = "555-0103", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Carol Smith", EmergencyContactPhone = "555-0104", JoinDate = today.AddMonths(-4) },
            new Member { FirstName = "Carol", LastName = "Davis", Email = "carol@example.com", Phone = "555-0105", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Dan Davis", EmergencyContactPhone = "555-0106", JoinDate = today.AddMonths(-3) },
            new Member { FirstName = "David", LastName = "Wilson", Email = "david@example.com", Phone = "555-0107", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Eve Wilson", EmergencyContactPhone = "555-0108", JoinDate = today.AddMonths(-5) },
            new Member { FirstName = "Emma", LastName = "Brown", Email = "emma@example.com", Phone = "555-0109", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Frank Brown", EmergencyContactPhone = "555-0110", JoinDate = today.AddMonths(-2) },
            new Member { FirstName = "Frank", LastName = "Taylor", Email = "frank@example.com", Phone = "555-0111", DateOfBirth = new DateOnly(1983, 9, 25), EmergencyContactName = "Grace Taylor", EmergencyContactPhone = "555-0112", JoinDate = today.AddMonths(-7) },
            new Member { FirstName = "Grace", LastName = "Anderson", Email = "grace@example.com", Phone = "555-0113", DateOfBirth = new DateOnly(1998, 2, 14), EmergencyContactName = "Hank Anderson", EmergencyContactPhone = "555-0114", JoinDate = today.AddMonths(-1) },
            new Member { FirstName = "Henry", LastName = "Martinez", Email = "henry@example.com", Phone = "555-0115", DateOfBirth = new DateOnly(1991, 6, 7), EmergencyContactName = "Ivy Martinez", EmergencyContactPhone = "555-0116", JoinDate = today.AddMonths(-3) },
        };
        db.Members.AddRange(members);
        db.SaveChanges();

        // ── Memberships ──
        var memberships = new[]
        {
            new Membership { MemberId = members[0].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[1].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[2].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[4].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[5].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            // Expired memberships
            new Membership { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new Membership { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
        };
        db.Memberships.AddRange(memberships);
        db.SaveChanges();

        // ── Instructors ──
        var instructors = new[]
        {
            new Instructor { FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@zenith.com", Phone = "555-0201", Bio = "15 years of yoga and meditation experience", Specializations = "Yoga,Meditation,Pilates", HireDate = new DateOnly(2020, 1, 15) },
            new Instructor { FirstName = "Mike", LastName = "Rodriguez", Email = "mike.rodriguez@zenith.com", Phone = "555-0202", Bio = "Former professional boxer and certified HIIT trainer", Specializations = "HIIT,Boxing,Strength", HireDate = new DateOnly(2019, 6, 1) },
            new Instructor { FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@zenith.com", Phone = "555-0203", Bio = "Certified spin instructor with a passion for high-energy workouts", Specializations = "Spin,HIIT,CrossFit", HireDate = new DateOnly(2021, 3, 10) },
            new Instructor { FirstName = "James", LastName = "Park", Email = "james.park@zenith.com", Phone = "555-0204", Bio = "Pilates and functional movement specialist", Specializations = "Pilates,Yoga,Flexibility", HireDate = new DateOnly(2022, 8, 20) },
        };
        db.Instructors.AddRange(instructors);
        db.SaveChanges();

        // ── Class Types ──
        var classTypes = new[]
        {
            new ClassType { Name = "Yoga", Description = "Vinyasa flow yoga for flexibility and mindfulness", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels },
            new ClassType { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate },
            new ClassType { Name = "Spin", Description = "Indoor cycling class with energizing music", DefaultDurationMinutes = 45, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate },
            new ClassType { Name = "Pilates", Description = "Core strengthening and body conditioning", DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner },
            new ClassType { Name = "Boxing", Description = "Premium boxing class with personal attention", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced },
            new ClassType { Name = "Meditation", Description = "Premium guided meditation and breathwork session", DefaultDurationMinutes = 30, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner },
        };
        db.ClassTypes.AddRange(classTypes);
        db.SaveChanges();

        // ── Class Schedules (next 7 days) ──
        var tomorrow = now.Date.AddDays(1);
        var schedules = new[]
        {
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, Room = "Studio A", CurrentEnrollment = 3 },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, Room = "Studio B", CurrentEnrollment = 5 },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddHours(11), EndTime = tomorrow.AddHours(11).AddMinutes(45), Capacity = 25, Room = "Spin Room", CurrentEnrollment = 2 },
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddHours(14), EndTime = tomorrow.AddHours(14).AddMinutes(50), Capacity = 15, Room = "Studio A", CurrentEnrollment = 4 },
            // Full class with waitlist
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11), Capacity = 3, Room = "Boxing Ring", CurrentEnrollment = 3, WaitlistCount = 2 },
            // Premium class
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(1).AddHours(17), EndTime = tomorrow.AddDays(1).AddHours(17).AddMinutes(30), Capacity = 12, Room = "Zen Room", CurrentEnrollment = 1 },
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8), Capacity = 20, Room = "Studio A", CurrentEnrollment = 0 },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(12).AddMinutes(45), Capacity = 15, Room = "Studio B", CurrentEnrollment = 0 },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(3).AddHours(9), EndTime = tomorrow.AddDays(3).AddHours(9).AddMinutes(45), Capacity = 25, Room = "Spin Room", CurrentEnrollment = 0 },
            // Cancelled class
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(3).AddHours(15), EndTime = tomorrow.AddDays(3).AddHours(15).AddMinutes(50), Capacity = 15, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(4).AddHours(10), EndTime = tomorrow.AddDays(4).AddHours(11), Capacity = 10, Room = "Boxing Ring", CurrentEnrollment = 0 },
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(5).AddHours(8), EndTime = tomorrow.AddDays(5).AddHours(9), Capacity = 20, Room = "Studio A", CurrentEnrollment = 0 },
        };
        db.ClassSchedules.AddRange(schedules);
        db.SaveChanges();

        // ── Bookings ──
        var bookings = new List<Booking>
        {
            // Confirmed bookings for schedule[0] (Yoga tomorrow)
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12) },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-10) },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8) },
            // Confirmed bookings for schedule[1] (HIIT tomorrow)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6) },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5) },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4) },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3) },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2) },
            // Spin class bookings
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-7) },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6) },
            // Full boxing class (premium) with waitlist
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-20) },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-18) },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-16) },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[5].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-14) },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-12) },
        };

        // Pilates bookings
        for (int i = 0; i < 4; i++)
            bookings.Add(new Booking { ClassScheduleId = schedules[3].Id, MemberId = members[i].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-11 + i) });

        // Meditation booking
        bookings.Add(new Booking { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1) });

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }
}
