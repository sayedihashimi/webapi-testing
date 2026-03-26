using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public sealed class DataSeeder
{
    public static async Task SeedAsync(SparkEventsDbContext db)
    {
        if (await db.EventCategories.AnyAsync())
            return;

        // Categories
        var categories = new List<EventCategory>
        {
            new() { Name = "Technology", Description = "Software development, AI, and emerging tech events", ColorHex = "#007BFF" },
            new() { Name = "Business", Description = "Entrepreneurship, leadership, and management events", ColorHex = "#28A745" },
            new() { Name = "Creative Arts", Description = "Design, music, photography, and creative workshops", ColorHex = "#FFC107" },
            new() { Name = "Health & Wellness", Description = "Fitness, mindfulness, and health-focused gatherings", ColorHex = "#DC3545" }
        };
        db.EventCategories.AddRange(categories);
        await db.SaveChangesAsync();

        // Venues
        var venues = new List<Venue>
        {
            new() { Name = "The Innovation Hub", Address = "123 Tech Boulevard", City = "Austin", State = "TX", ZipCode = "73301", MaxCapacity = 50, ContactEmail = "info@innovationhub.com", ContactPhone = "512-555-0101", Notes = "Small venue with modern AV setup", CreatedAt = DateTime.UtcNow },
            new() { Name = "Downtown Convention Center", Address = "500 Congress Avenue", City = "Austin", State = "TX", ZipCode = "73301", MaxCapacity = 200, ContactEmail = "events@downtowncc.com", ContactPhone = "512-555-0202", Notes = "Medium venue with multiple breakout rooms", CreatedAt = DateTime.UtcNow },
            new() { Name = "Grand Exhibition Hall", Address = "1000 Exposition Blvd", City = "Austin", State = "TX", ZipCode = "73344", MaxCapacity = 500, ContactEmail = "bookings@grandhall.com", ContactPhone = "512-555-0303", Notes = "Large venue for conferences and expos", CreatedAt = DateTime.UtcNow }
        };
        db.Venues.AddRange(venues);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // Events
        // Event 1: Upcoming published with capacity
        var event1 = new Event
        {
            Title = "AI & Machine Learning Summit 2026",
            Description = "Join industry leaders for a deep dive into the latest AI and ML technologies. Featuring keynotes, hands-on workshops, and networking opportunities.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[2].Id,
            StartDate = now.AddDays(14),
            EndDate = now.AddDays(14).AddHours(8),
            RegistrationOpenDate = now.AddDays(-30),
            RegistrationCloseDate = now.AddDays(13),
            EarlyBirdDeadline = now.AddDays(-5),
            TotalCapacity = 300,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = true,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        };

        // Event 2: Upcoming published with capacity
        var event2 = new Event
        {
            Title = "Startup Pitch Night",
            Description = "Watch 10 startups pitch their ideas to a panel of investors. Network with founders and VCs in our intimate venue setting.",
            EventCategoryId = categories[1].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(7),
            EndDate = now.AddDays(7).AddHours(3),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.AddDays(6),
            EarlyBirdDeadline = now.AddDays(-3),
            TotalCapacity = 45,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-14),
            UpdatedAt = now
        };

        // Event 3: Sold out with waitlist
        var event3 = new Event
        {
            Title = "UX Design Masterclass",
            Description = "An intensive workshop on user experience design principles, prototyping, and user testing methodologies.",
            EventCategoryId = categories[2].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(10),
            EndDate = now.AddDays(10).AddHours(6),
            RegistrationOpenDate = now.AddDays(-20),
            RegistrationCloseDate = now.AddDays(9),
            TotalCapacity = 30,
            CurrentRegistrations = 30,
            WaitlistCount = 0,
            Status = EventStatus.SoldOut,
            IsFeatured = true,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now
        };

        // Event 4: Today/tomorrow for check-in testing
        var event4 = new Event
        {
            Title = "Morning Yoga & Mindfulness Retreat",
            Description = "Start your day with guided yoga and meditation sessions. All experience levels welcome.",
            EventCategoryId = categories[3].Id,
            VenueId = venues[0].Id,
            StartDate = now.Date.AddHours(8),
            EndDate = now.Date.AddHours(17),
            RegistrationOpenDate = now.AddDays(-10),
            RegistrationCloseDate = now.Date.AddHours(-1),
            TotalCapacity = 40,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now
        };

        // Event 5: Completed past event
        var event5 = new Event
        {
            Title = "Cloud Architecture Workshop",
            Description = "Hands-on workshop covering cloud-native architecture patterns, microservices, and containerization.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[1].Id,
            StartDate = now.AddDays(-14),
            EndDate = now.AddDays(-14).AddHours(8),
            RegistrationOpenDate = now.AddDays(-45),
            RegistrationCloseDate = now.AddDays(-15),
            EarlyBirdDeadline = now.AddDays(-30),
            TotalCapacity = 100,
            CurrentRegistrations = 45,
            WaitlistCount = 0,
            Status = EventStatus.Completed,
            IsFeatured = false,
            CreatedAt = now.AddDays(-45),
            UpdatedAt = now.AddDays(-14)
        };

        // Event 6: Draft event
        var event6 = new Event
        {
            Title = "Leadership in Tech Conference 2026",
            Description = "A two-day conference exploring leadership challenges in the technology sector. Featuring panels, workshops, and keynotes from industry executives.",
            EventCategoryId = categories[1].Id,
            VenueId = venues[2].Id,
            StartDate = now.AddDays(60),
            EndDate = now.AddDays(61),
            RegistrationOpenDate = now.AddDays(30),
            RegistrationCloseDate = now.AddDays(59),
            EarlyBirdDeadline = now.AddDays(45),
            TotalCapacity = 400,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Draft,
            IsFeatured = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var events = new List<Event> { event1, event2, event3, event4, event5, event6 };
        db.Events.AddRange(events);
        await db.SaveChangesAsync();

        // Ticket Types (3 per event)
        var ticketTypes = new List<TicketType>();
        foreach (var evt in events)
        {
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "General Admission", Description = "Standard entry ticket", Price = 25.00m, EarlyBirdPrice = 15.00m, Quantity = (int)(evt.TotalCapacity * 0.6), SortOrder = 1, IsActive = true, CreatedAt = now });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "VIP", Description = "Premium seating, meet & greet, swag bag", Price = 75.00m, EarlyBirdPrice = 55.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 2, IsActive = true, CreatedAt = now });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "Student", Description = "Discounted ticket for students with valid ID", Price = 10.00m, EarlyBirdPrice = 5.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 3, IsActive = true, CreatedAt = now });
        }
        db.TicketTypes.AddRange(ticketTypes);
        await db.SaveChangesAsync();

        // Attendees
        var attendees = new List<Attendee>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "512-555-1001", Organization = "TechStart Inc.", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Bob", LastName = "Williams", Email = "bob.williams@example.com", Phone = "512-555-1002", Organization = "Design Co.", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Carol", LastName = "Davis", Email = "carol.davis@example.com", Phone = "512-555-1003", Organization = "University of Texas", DietaryNeeds = "Vegetarian", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Martinez", Email = "david.martinez@example.com", Phone = "512-555-1004", Organization = "Innovate Labs", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Brown", Email = "emma.brown@example.com", Phone = "512-555-1005", Organization = "CloudScale Solutions", DietaryNeeds = "Gluten-free", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Frank", LastName = "Garcia", Email = "frank.garcia@example.com", Phone = "512-555-1006", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Grace", LastName = "Lee", Email = "grace.lee@example.com", Phone = "512-555-1007", Organization = "Creative Studio", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Henry", LastName = "Wilson", Email = "henry.wilson@example.com", Phone = "512-555-1008", Organization = "HealthFirst Wellness", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Isabella", LastName = "Anderson", Email = "isabella.anderson@example.com", Phone = "512-555-1009", Organization = "DataDriven Corp", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jack", LastName = "Thomas", Email = "jack.thomas@example.com", Phone = "512-555-1010", Organization = "StartUp Weekend", DietaryNeeds = "Vegan", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Karen", LastName = "Taylor", Email = "karen.taylor@example.com", Phone = "512-555-1011", Organization = "MindBody Yoga", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Liam", LastName = "Moore", Email = "liam.moore@example.com", Phone = "512-555-1012", Organization = "DevOps United", CreatedAt = now, UpdatedAt = now }
        };
        db.Attendees.AddRange(attendees);
        await db.SaveChangesAsync();

        // Registrations - need to get ticket types by event
        var registrations = new List<Registration>();
        var regCounter = new Dictionary<int, int>();

        Registration MakeReg(int eventId, int attendeeId, int ticketTypeId, RegistrationStatus status, DateTime regDate, decimal amount, int? waitlistPos = null)
        {
            if (!regCounter.ContainsKey(eventId)) regCounter[eventId] = 0;
            regCounter[eventId]++;
            var evt = events.First(e => e.Id == eventId);
            var confNum = $"SPK-{evt.StartDate:yyyyMMdd}-{regCounter[eventId]:D4}";

            return new Registration
            {
                EventId = eventId,
                AttendeeId = attendeeId,
                TicketTypeId = ticketTypeId,
                ConfirmationNumber = confNum,
                Status = status,
                AmountPaid = amount,
                WaitlistPosition = waitlistPos,
                RegistrationDate = regDate,
                CheckInTime = status == RegistrationStatus.CheckedIn ? regDate.AddHours(2) : null,
                CancellationDate = status == RegistrationStatus.Cancelled ? regDate.AddDays(1) : null,
                CancellationReason = status == RegistrationStatus.Cancelled ? "Schedule conflict" : null,
                CreatedAt = regDate,
                UpdatedAt = regDate
            };
        }

        // Get ticket type IDs per event
        var e1Tickets = ticketTypes.Where(t => t.EventId == event1.Id).ToList();
        var e2Tickets = ticketTypes.Where(t => t.EventId == event2.Id).ToList();
        var e3Tickets = ticketTypes.Where(t => t.EventId == event3.Id).ToList();
        var e4Tickets = ticketTypes.Where(t => t.EventId == event4.Id).ToList();
        var e5Tickets = ticketTypes.Where(t => t.EventId == event5.Id).ToList();

        // Event 1 (AI Summit) - several confirmed, one cancelled
        registrations.Add(MakeReg(event1.Id, attendees[0].Id, e1Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-10), 15.00m));
        registrations.Add(MakeReg(event1.Id, attendees[1].Id, e1Tickets[1].Id, RegistrationStatus.Confirmed, now.AddDays(-9), 55.00m));
        registrations.Add(MakeReg(event1.Id, attendees[2].Id, e1Tickets[2].Id, RegistrationStatus.Confirmed, now.AddDays(-8), 5.00m));
        registrations.Add(MakeReg(event1.Id, attendees[3].Id, e1Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-2), 25.00m));
        registrations.Add(MakeReg(event1.Id, attendees[8].Id, e1Tickets[0].Id, RegistrationStatus.Cancelled, now.AddDays(-7), 15.00m));

        // Event 2 (Startup Pitch Night) - a few registrations
        registrations.Add(MakeReg(event2.Id, attendees[4].Id, e2Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-5), 15.00m));
        registrations.Add(MakeReg(event2.Id, attendees[5].Id, e2Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-4), 15.00m));
        registrations.Add(MakeReg(event2.Id, attendees[9].Id, e2Tickets[1].Id, RegistrationStatus.Confirmed, now.AddDays(-3), 55.00m));

        // Event 3 (UX Design - SoldOut) - fill it up and add waitlist
        // We won't actually create 30 registrations, but simulate it by using existing attendees
        registrations.Add(MakeReg(event3.Id, attendees[0].Id, e3Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-15), 25.00m));
        registrations.Add(MakeReg(event3.Id, attendees[1].Id, e3Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-14), 25.00m));
        registrations.Add(MakeReg(event3.Id, attendees[2].Id, e3Tickets[1].Id, RegistrationStatus.Confirmed, now.AddDays(-13), 75.00m));
        registrations.Add(MakeReg(event3.Id, attendees[6].Id, e3Tickets[0].Id, RegistrationStatus.Waitlisted, now.AddDays(-5), 25.00m, 1));
        registrations.Add(MakeReg(event3.Id, attendees[7].Id, e3Tickets[0].Id, RegistrationStatus.Waitlisted, now.AddDays(-4), 25.00m, 2));

        // Update event3 waitlist count
        event3.WaitlistCount = 2;

        // Event 4 (Yoga - today) - confirmed and checked-in
        registrations.Add(MakeReg(event4.Id, attendees[7].Id, e4Tickets[0].Id, RegistrationStatus.CheckedIn, now.AddDays(-5), 25.00m));
        registrations.Add(MakeReg(event4.Id, attendees[10].Id, e4Tickets[0].Id, RegistrationStatus.CheckedIn, now.AddDays(-4), 25.00m));
        registrations.Add(MakeReg(event4.Id, attendees[11].Id, e4Tickets[2].Id, RegistrationStatus.Confirmed, now.AddDays(-3), 10.00m));
        registrations.Add(MakeReg(event4.Id, attendees[3].Id, e4Tickets[0].Id, RegistrationStatus.Confirmed, now.AddDays(-2), 25.00m));

        // Update event4 registration count
        event4.CurrentRegistrations = 4;

        // Event 5 (Cloud Workshop - completed) - confirmed and checked-in
        registrations.Add(MakeReg(event5.Id, attendees[0].Id, e5Tickets[0].Id, RegistrationStatus.CheckedIn, now.AddDays(-30), 15.00m));
        registrations.Add(MakeReg(event5.Id, attendees[3].Id, e5Tickets[1].Id, RegistrationStatus.CheckedIn, now.AddDays(-29), 55.00m));
        registrations.Add(MakeReg(event5.Id, attendees[4].Id, e5Tickets[0].Id, RegistrationStatus.CheckedIn, now.AddDays(-28), 15.00m));
        registrations.Add(MakeReg(event5.Id, attendees[11].Id, e5Tickets[2].Id, RegistrationStatus.NoShow, now.AddDays(-27), 5.00m));

        // Update event1 registration count
        event1.CurrentRegistrations = 4;
        // Update event2 registration count
        event2.CurrentRegistrations = 3;

        // Update ticket type sold counts
        e1Tickets[0].QuantitySold = 2; // 2 confirmed GA (one cancelled doesn't count)
        e1Tickets[1].QuantitySold = 1;
        e1Tickets[2].QuantitySold = 1;
        e2Tickets[0].QuantitySold = 2;
        e2Tickets[1].QuantitySold = 1;
        e4Tickets[0].QuantitySold = 3;
        e4Tickets[2].QuantitySold = 1;

        db.Registrations.AddRange(registrations);
        await db.SaveChangesAsync();

        // Check-ins for event4 and event5
        var checkIns = new List<CheckIn>();
        var checkedInRegs = registrations.Where(r => r.Status == RegistrationStatus.CheckedIn).ToList();
        foreach (var reg in checkedInRegs)
        {
            checkIns.Add(new CheckIn
            {
                RegistrationId = reg.Id,
                CheckInTime = reg.CheckInTime ?? DateTime.UtcNow,
                CheckedInBy = "Staff Member",
                Notes = null
            });
        }
        db.CheckIns.AddRange(checkIns);
        await db.SaveChangesAsync();
    }
}
