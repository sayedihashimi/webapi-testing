using SparkEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace SparkEvents.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SparkEventsDbContext context)
    {
        if (await context.EventCategories.AnyAsync())
            return;

        // Categories
        var categories = new List<EventCategory>
        {
            new() { Name = "Technology", Description = "Tech conferences, coding workshops, and hackathons", ColorHex = "#007BFF" },
            new() { Name = "Business", Description = "Networking events, seminars, and entrepreneurship workshops", ColorHex = "#28A745" },
            new() { Name = "Creative Arts", Description = "Art shows, music events, and creative workshops", ColorHex = "#FFC107" },
            new() { Name = "Health & Wellness", Description = "Fitness classes, wellness seminars, and health fairs", ColorHex = "#DC3545" }
        };
        context.EventCategories.AddRange(categories);
        await context.SaveChangesAsync();

        // Venues
        var venues = new List<Venue>
        {
            new() { Name = "The Innovation Hub", Address = "123 Tech Lane", City = "Austin", State = "TX", ZipCode = "78701", MaxCapacity = 50, ContactEmail = "info@innovationhub.com", ContactPhone = "512-555-0101", Notes = "Small intimate venue with AV setup" },
            new() { Name = "Grand Convention Center", Address = "500 Main Street", City = "Dallas", State = "TX", ZipCode = "75201", MaxCapacity = 200, ContactEmail = "events@grandcc.com", ContactPhone = "214-555-0202", Notes = "Medium venue with multiple breakout rooms" },
            new() { Name = "Metro Arena", Address = "1000 Stadium Drive", City = "Houston", State = "TX", ZipCode = "77002", MaxCapacity = 500, ContactEmail = "booking@metroarena.com", ContactPhone = "713-555-0303", Notes = "Large venue for major events" }
        };
        context.Venues.AddRange(venues);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // Events
        // Event 1: Upcoming Published - Tech Conference (future, capacity available)
        var event1 = new Event
        {
            Title = "Austin Tech Summit 2026",
            Description = "A premier technology conference featuring keynote speakers, workshops, and networking. Join industry leaders to explore the latest in AI, cloud computing, and software engineering.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[1].Id,
            StartDate = now.AddDays(14),
            EndDate = now.AddDays(14).AddHours(8),
            RegistrationOpenDate = now.AddDays(-30),
            RegistrationCloseDate = now.AddDays(13),
            EarlyBirdDeadline = now.AddDays(-5),
            TotalCapacity = 150,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = true
        };

        // Event 2: Upcoming Published - Business Networking
        var event2 = new Event
        {
            Title = "Entrepreneur Meetup & Pitch Night",
            Description = "Connect with fellow entrepreneurs, angel investors, and mentors. Pitch your startup idea and get real feedback in a supportive environment.",
            EventCategoryId = categories[1].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(7),
            EndDate = now.AddDays(7).AddHours(4),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.AddDays(6),
            EarlyBirdDeadline = now.AddDays(-3),
            TotalCapacity = 40,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false
        };

        // Event 3: SoldOut - Creative Workshop
        var event3 = new Event
        {
            Title = "Digital Art Masterclass",
            Description = "An intensive hands-on workshop covering digital illustration techniques, color theory, and composition. Bring your own tablet or laptop.",
            EventCategoryId = categories[2].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(10),
            EndDate = now.AddDays(10).AddHours(6),
            RegistrationOpenDate = now.AddDays(-21),
            RegistrationCloseDate = now.AddDays(9),
            TotalCapacity = 20,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.SoldOut,
            IsFeatured = true
        };

        // Event 4: Today/Tomorrow - for check-in testing
        var event4 = new Event
        {
            Title = "Morning Yoga & Mindfulness Retreat",
            Description = "Start your day with guided yoga sessions and mindfulness meditation. Suitable for all levels from beginners to advanced practitioners.",
            EventCategoryId = categories[3].Id,
            VenueId = venues[0].Id,
            StartDate = now.Date.AddHours(8),
            EndDate = now.Date.AddHours(17),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.Date.AddHours(-1),
            TotalCapacity = 30,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false
        };

        // Event 5: Completed (past)
        var event5 = new Event
        {
            Title = "Cloud Architecture Workshop 2026",
            Description = "A deep-dive workshop on cloud architecture patterns, microservices, and DevOps practices. Hands-on labs with AWS and Azure.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[1].Id,
            StartDate = now.AddDays(-14),
            EndDate = now.AddDays(-14).AddHours(8),
            RegistrationOpenDate = now.AddDays(-45),
            RegistrationCloseDate = now.AddDays(-15),
            EarlyBirdDeadline = now.AddDays(-30),
            TotalCapacity = 100,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Completed,
            IsFeatured = false
        };

        // Event 6: Draft
        var event6 = new Event
        {
            Title = "Startup Bootcamp: From Idea to MVP",
            Description = "An intensive weekend bootcamp for aspiring entrepreneurs. Learn lean methodology, customer discovery, and rapid prototyping.",
            EventCategoryId = categories[1].Id,
            VenueId = venues[2].Id,
            StartDate = now.AddDays(45),
            EndDate = now.AddDays(47),
            RegistrationOpenDate = now.AddDays(14),
            RegistrationCloseDate = now.AddDays(44),
            EarlyBirdDeadline = now.AddDays(30),
            TotalCapacity = 200,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Draft,
            IsFeatured = false
        };

        var events = new List<Event> { event1, event2, event3, event4, event5, event6 };
        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        // Ticket Types (3 per event)
        var allTicketTypes = new List<TicketType>();
        foreach (var evt in events)
        {
            var ga = new TicketType { EventId = evt.Id, Name = "General Admission", Description = "Standard entry with access to all sessions", Price = 25.00m, EarlyBirdPrice = 15.00m, Quantity = (int)(evt.TotalCapacity * 0.6), SortOrder = 1 };
            var vip = new TicketType { EventId = evt.Id, Name = "VIP", Description = "Premium entry with front-row seating and exclusive networking", Price = 75.00m, EarlyBirdPrice = 55.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 2 };
            var student = new TicketType { EventId = evt.Id, Name = "Student", Description = "Discounted entry for students with valid ID", Price = 10.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 3 };
            allTicketTypes.AddRange(new[] { ga, vip, student });
        }
        context.TicketTypes.AddRange(allTicketTypes);
        await context.SaveChangesAsync();

        // Attendees (12)
        var attendees = new List<Attendee>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "512-555-1001", Organization = "TechCorp", DietaryNeeds = "Vegetarian" },
            new() { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "214-555-1002", Organization = "StartupXYZ" },
            new() { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "713-555-1003", Organization = "DesignStudio" },
            new() { FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Phone = "512-555-1004", Organization = "CloudNine Inc.", DietaryNeeds = "Gluten-free" },
            new() { FirstName = "Elena", LastName = "Martinez", Email = "elena.martinez@example.com", Phone = "214-555-1005", Organization = "InnoVentures" },
            new() { FirstName = "Frank", LastName = "Davis", Email = "frank.davis@example.com", Organization = "University of Texas" },
            new() { FirstName = "Grace", LastName = "Lee", Email = "grace.lee@example.com", Phone = "713-555-1007", Organization = "HealthFirst" },
            new() { FirstName = "Henry", LastName = "Wilson", Email = "henry.wilson@example.com", Phone = "512-555-1008" },
            new() { FirstName = "Iris", LastName = "Chen", Email = "iris.chen@example.com", Phone = "214-555-1009", Organization = "ArtSpace Gallery", DietaryNeeds = "Vegan" },
            new() { FirstName = "Jack", LastName = "Taylor", Email = "jack.taylor@example.com", Organization = "FreelanceDevs" },
            new() { FirstName = "Karen", LastName = "Anderson", Email = "karen.anderson@example.com", Phone = "713-555-1011", Organization = "WellnessWorks" },
            new() { FirstName = "Leo", LastName = "Thomas", Email = "leo.thomas@example.com", Phone = "512-555-1012", Organization = "DataDriven LLC" }
        };
        context.Attendees.AddRange(attendees);
        await context.SaveChangesAsync();

        // Helper to create confirmation numbers
        int regCounter = 1;
        string MakeConfNum(Event evt) => $"SPK-{evt.StartDate:yyyyMMdd}-{regCounter++:D4}";

        var registrations = new List<Registration>();

        // Get ticket types by event
        var e1Tickets = allTicketTypes.Where(t => t.EventId == event1.Id).ToList();
        var e2Tickets = allTicketTypes.Where(t => t.EventId == event2.Id).ToList();
        var e3Tickets = allTicketTypes.Where(t => t.EventId == event3.Id).ToList();
        var e4Tickets = allTicketTypes.Where(t => t.EventId == event4.Id).ToList();
        var e5Tickets = allTicketTypes.Where(t => t.EventId == event5.Id).ToList();

        // Event 1 registrations (4 confirmed, 1 cancelled) - Tech Summit
        registrations.Add(new Registration { EventId = event1.Id, AttendeeId = attendees[0].Id, TicketTypeId = e1Tickets[0].Id, ConfirmationNumber = MakeConfNum(event1), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-20) });
        registrations.Add(new Registration { EventId = event1.Id, AttendeeId = attendees[1].Id, TicketTypeId = e1Tickets[1].Id, ConfirmationNumber = MakeConfNum(event1), Status = RegistrationStatus.Confirmed, AmountPaid = 55.00m, RegistrationDate = now.AddDays(-18) });
        registrations.Add(new Registration { EventId = event1.Id, AttendeeId = attendees[3].Id, TicketTypeId = e1Tickets[0].Id, ConfirmationNumber = MakeConfNum(event1), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-15) });
        registrations.Add(new Registration { EventId = event1.Id, AttendeeId = attendees[11].Id, TicketTypeId = e1Tickets[2].Id, ConfirmationNumber = MakeConfNum(event1), Status = RegistrationStatus.Confirmed, AmountPaid = 10.00m, RegistrationDate = now.AddDays(-10) });
        registrations.Add(new Registration { EventId = event1.Id, AttendeeId = attendees[4].Id, TicketTypeId = e1Tickets[0].Id, ConfirmationNumber = MakeConfNum(event1), Status = RegistrationStatus.Cancelled, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-8), CancellationDate = now.AddDays(-5), CancellationReason = "Schedule conflict" });

        // Event 2 registrations (3 confirmed)
        registrations.Add(new Registration { EventId = event2.Id, AttendeeId = attendees[4].Id, TicketTypeId = e2Tickets[0].Id, ConfirmationNumber = MakeConfNum(event2), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-10) });
        registrations.Add(new Registration { EventId = event2.Id, AttendeeId = attendees[5].Id, TicketTypeId = e2Tickets[2].Id, ConfirmationNumber = MakeConfNum(event2), Status = RegistrationStatus.Confirmed, AmountPaid = 10.00m, RegistrationDate = now.AddDays(-9) });
        registrations.Add(new Registration { EventId = event2.Id, AttendeeId = attendees[7].Id, TicketTypeId = e2Tickets[0].Id, ConfirmationNumber = MakeConfNum(event2), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-3) });

        // Event 3 registrations - SoldOut (fill to capacity=20, plus 2 waitlisted)
        // Using GA (qty 12), VIP (qty 4), Student (qty 4) = 20 total capacity
        // Fill all 20 confirmed spots
        var e3AttendeeIds = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        for (int i = 0; i < 10; i++)
        {
            var ticketIdx = i < 6 ? 0 : (i < 8 ? 1 : 2); // 6 GA, 2 VIP, 2 Student
            registrations.Add(new Registration
            {
                EventId = event3.Id,
                AttendeeId = attendees[e3AttendeeIds[i]].Id,
                TicketTypeId = e3Tickets[ticketIdx].Id,
                ConfirmationNumber = MakeConfNum(event3),
                Status = RegistrationStatus.Confirmed,
                AmountPaid = e3Tickets[ticketIdx].Price,
                RegistrationDate = now.AddDays(-15 + i)
            });
        }
        // Additional confirmed to fill capacity (use remaining capacity with new regs for existing attendees won't work - duplicate rule)
        // Let's just set capacity counts to match 10 confirmed + mark the rest via direct count update

        // 2 waitlisted
        registrations.Add(new Registration { EventId = event3.Id, AttendeeId = attendees[10].Id, TicketTypeId = e3Tickets[0].Id, ConfirmationNumber = MakeConfNum(event3), Status = RegistrationStatus.Waitlisted, AmountPaid = 25.00m, WaitlistPosition = 1, RegistrationDate = now.AddDays(-4) });
        registrations.Add(new Registration { EventId = event3.Id, AttendeeId = attendees[11].Id, TicketTypeId = e3Tickets[0].Id, ConfirmationNumber = MakeConfNum(event3), Status = RegistrationStatus.Waitlisted, AmountPaid = 25.00m, WaitlistPosition = 2, RegistrationDate = now.AddDays(-3) });

        // Event 4 registrations - Today event (5 confirmed, some checked in)
        registrations.Add(new Registration { EventId = event4.Id, AttendeeId = attendees[0].Id, TicketTypeId = e4Tickets[0].Id, ConfirmationNumber = MakeConfNum(event4), Status = RegistrationStatus.CheckedIn, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-7), CheckInTime = now.Date.AddHours(8).AddMinutes(15) });
        registrations.Add(new Registration { EventId = event4.Id, AttendeeId = attendees[6].Id, TicketTypeId = e4Tickets[0].Id, ConfirmationNumber = MakeConfNum(event4), Status = RegistrationStatus.CheckedIn, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-6), CheckInTime = now.Date.AddHours(8).AddMinutes(20) });
        registrations.Add(new Registration { EventId = event4.Id, AttendeeId = attendees[10].Id, TicketTypeId = e4Tickets[1].Id, ConfirmationNumber = MakeConfNum(event4), Status = RegistrationStatus.Confirmed, AmountPaid = 75.00m, RegistrationDate = now.AddDays(-5) });
        registrations.Add(new Registration { EventId = event4.Id, AttendeeId = attendees[2].Id, TicketTypeId = e4Tickets[2].Id, ConfirmationNumber = MakeConfNum(event4), Status = RegistrationStatus.Confirmed, AmountPaid = 10.00m, RegistrationDate = now.AddDays(-4) });
        registrations.Add(new Registration { EventId = event4.Id, AttendeeId = attendees[8].Id, TicketTypeId = e4Tickets[0].Id, ConfirmationNumber = MakeConfNum(event4), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-3) });

        // Event 5 registrations - Completed (4 checked in, 1 no-show)
        registrations.Add(new Registration { EventId = event5.Id, AttendeeId = attendees[0].Id, TicketTypeId = e5Tickets[0].Id, ConfirmationNumber = MakeConfNum(event5), Status = RegistrationStatus.CheckedIn, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-40), CheckInTime = now.AddDays(-14).AddHours(1) });
        registrations.Add(new Registration { EventId = event5.Id, AttendeeId = attendees[1].Id, TicketTypeId = e5Tickets[1].Id, ConfirmationNumber = MakeConfNum(event5), Status = RegistrationStatus.CheckedIn, AmountPaid = 55.00m, RegistrationDate = now.AddDays(-38), CheckInTime = now.AddDays(-14).AddHours(1).AddMinutes(10) });
        registrations.Add(new Registration { EventId = event5.Id, AttendeeId = attendees[3].Id, TicketTypeId = e5Tickets[0].Id, ConfirmationNumber = MakeConfNum(event5), Status = RegistrationStatus.CheckedIn, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-35), CheckInTime = now.AddDays(-14).AddHours(1).AddMinutes(20) });
        registrations.Add(new Registration { EventId = event5.Id, AttendeeId = attendees[5].Id, TicketTypeId = e5Tickets[2].Id, ConfirmationNumber = MakeConfNum(event5), Status = RegistrationStatus.NoShow, AmountPaid = 10.00m, RegistrationDate = now.AddDays(-33) });

        context.Registrations.AddRange(registrations);
        await context.SaveChangesAsync();

        // Update counts on events
        foreach (var evt in events)
        {
            var eventRegs = registrations.Where(r => r.EventId == evt.Id).ToList();
            evt.CurrentRegistrations = eventRegs.Count(r => r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn);
            evt.WaitlistCount = eventRegs.Count(r => r.Status == RegistrationStatus.Waitlisted);
        }

        // Update ticket type sold counts
        foreach (var tt in allTicketTypes)
        {
            tt.QuantitySold = registrations.Count(r => r.TicketTypeId == tt.Id && r.Status != RegistrationStatus.Cancelled && r.Status != RegistrationStatus.Waitlisted);
        }

        // For Event 3 (SoldOut), set TotalCapacity to match what we have confirmed to ensure SoldOut makes sense
        event3.TotalCapacity = 10; // We have exactly 10 confirmed
        // Update ticket quantities to match
        e3Tickets[0].Quantity = 6;
        e3Tickets[1].Quantity = 2;
        e3Tickets[2].Quantity = 2;

        await context.SaveChangesAsync();

        // Check-ins for today's event and completed event
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

        context.CheckIns.AddRange(checkIns);
        await context.SaveChangesAsync();
    }
}
