using SparkEvents.Models;

namespace SparkEvents.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SparkEventsDbContext db)
    {
        if (db.EventCategories.Any()) return;

        var now = DateTime.UtcNow;

        // Categories
        var categories = new[]
        {
            new EventCategory { Name = "Technology", Description = "Tech conferences, hackathons, and coding workshops", ColorHex = "#007BFF" },
            new EventCategory { Name = "Business", Description = "Networking events, seminars, and professional development", ColorHex = "#28A745" },
            new EventCategory { Name = "Creative Arts", Description = "Art exhibitions, music, and creative workshops", ColorHex = "#FFC107" },
            new EventCategory { Name = "Health & Wellness", Description = "Fitness classes, wellness retreats, and health seminars", ColorHex = "#DC3545" },
        };
        db.EventCategories.AddRange(categories);
        await db.SaveChangesAsync();

        // Venues
        var venues = new[]
        {
            new Venue
            {
                Name = "Innovation Hub", Address = "100 Tech Drive", City = "Austin", State = "TX", ZipCode = "78701",
                MaxCapacity = 500, ContactEmail = "events@innovationhub.com", ContactPhone = "512-555-0100",
                Notes = "Large conference center with breakout rooms", CreatedAt = now
            },
            new Venue
            {
                Name = "Downtown Conference Center", Address = "250 Main Street", City = "Austin", State = "TX", ZipCode = "78702",
                MaxCapacity = 200, ContactEmail = "booking@downtowncc.com", ContactPhone = "512-555-0200",
                Notes = "Medium-sized venue with modern AV equipment", CreatedAt = now
            },
            new Venue
            {
                Name = "Community Studio", Address = "42 Oak Lane", City = "Austin", State = "TX", ZipCode = "78703",
                MaxCapacity = 50, ContactEmail = "hello@communitystudio.com", ContactPhone = "512-555-0300",
                Notes = "Intimate space for workshops and small gatherings", CreatedAt = now
            },
        };
        db.Venues.AddRange(venues);
        await db.SaveChangesAsync();

        // Events
        var events = new List<Event>
        {
            // 1: Upcoming published with capacity
            new Event
            {
                Title = "Austin Tech Summit 2026", Description = "The premier technology conference in Austin featuring keynotes from industry leaders, hands-on workshops, and networking opportunities.",
                EventCategoryId = categories[0].Id, VenueId = venues[0].Id,
                StartDate = now.AddDays(14), EndDate = now.AddDays(14).AddHours(8),
                RegistrationOpenDate = now.AddDays(-30), RegistrationCloseDate = now.AddDays(13),
                EarlyBirdDeadline = now.AddDays(-5), TotalCapacity = 300,
                CurrentRegistrations = 0, WaitlistCount = 0, Status = EventStatus.Published, IsFeatured = true,
                CreatedAt = now.AddDays(-35), UpdatedAt = now
            },
            // 2: Upcoming published with capacity
            new Event
            {
                Title = "Startup Pitch Night", Description = "An evening of startup pitches, investor networking, and entrepreneurial inspiration.",
                EventCategoryId = categories[1].Id, VenueId = venues[1].Id,
                StartDate = now.AddDays(10), EndDate = now.AddDays(10).AddHours(4),
                RegistrationOpenDate = now.AddDays(-20), RegistrationCloseDate = now.AddDays(9),
                EarlyBirdDeadline = now.AddDays(-3), TotalCapacity = 150,
                CurrentRegistrations = 0, WaitlistCount = 0, Status = EventStatus.Published,
                CreatedAt = now.AddDays(-25), UpdatedAt = now
            },
            // 3: SoldOut with waitlist
            new Event
            {
                Title = "Watercolor Workshop", Description = "Learn the fundamentals of watercolor painting in this intimate hands-on workshop led by local artist Maria Chen.",
                EventCategoryId = categories[2].Id, VenueId = venues[2].Id,
                StartDate = now.AddDays(7), EndDate = now.AddDays(7).AddHours(3),
                RegistrationOpenDate = now.AddDays(-30), RegistrationCloseDate = now.AddDays(6),
                TotalCapacity = 20, CurrentRegistrations = 20, WaitlistCount = 3,
                Status = EventStatus.SoldOut,
                CreatedAt = now.AddDays(-35), UpdatedAt = now
            },
            // 4: Today/tomorrow for check-in testing
            new Event
            {
                Title = "Morning Yoga & Mindfulness", Description = "Start your day with an energizing yoga session followed by guided mindfulness meditation.",
                EventCategoryId = categories[3].Id, VenueId = venues[2].Id,
                StartDate = now.Date.AddHours(8), EndDate = now.Date.AddHours(18),
                RegistrationOpenDate = now.AddDays(-14), RegistrationCloseDate = now.Date.AddHours(-1),
                TotalCapacity = 30, CurrentRegistrations = 8, WaitlistCount = 0,
                Status = EventStatus.Published,
                CreatedAt = now.AddDays(-15), UpdatedAt = now
            },
            // 5: Completed
            new Event
            {
                Title = "AI & Machine Learning Conference", Description = "A full-day conference exploring the latest advances in AI, ML, and data science.",
                EventCategoryId = categories[0].Id, VenueId = venues[0].Id,
                StartDate = now.AddDays(-14), EndDate = now.AddDays(-14).AddHours(8),
                RegistrationOpenDate = now.AddDays(-60), RegistrationCloseDate = now.AddDays(-15),
                EarlyBirdDeadline = now.AddDays(-30), TotalCapacity = 400,
                CurrentRegistrations = 250, WaitlistCount = 0, Status = EventStatus.Completed,
                CreatedAt = now.AddDays(-65), UpdatedAt = now.AddDays(-14)
            },
            // 6: Draft
            new Event
            {
                Title = "Creative Writing Workshop (Draft)", Description = "A workshop for aspiring writers to develop their craft with guidance from published authors.",
                EventCategoryId = categories[2].Id, VenueId = venues[1].Id,
                StartDate = now.AddDays(45), EndDate = now.AddDays(45).AddHours(6),
                RegistrationOpenDate = now.AddDays(20), RegistrationCloseDate = now.AddDays(44),
                TotalCapacity = 60, CurrentRegistrations = 0, WaitlistCount = 0,
                Status = EventStatus.Draft,
                CreatedAt = now.AddDays(-5), UpdatedAt = now
            },
        };
        db.Events.AddRange(events);
        await db.SaveChangesAsync();

        // Ticket types (3 per event)
        var ticketTypes = new List<TicketType>();
        foreach (var evt in events)
        {
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "General Admission", Price = 25m, EarlyBirdPrice = 15m, Quantity = (int)(evt.TotalCapacity * 0.6), SortOrder = 1, IsActive = true, CreatedAt = now });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "VIP", Description = "Includes priority seating and exclusive networking", Price = 75m, EarlyBirdPrice = 50m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 2, IsActive = true, CreatedAt = now });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "Student", Description = "Valid student ID required", Price = 10m, EarlyBirdPrice = 5m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 3, IsActive = true, CreatedAt = now });
        }
        db.TicketTypes.AddRange(ticketTypes);
        await db.SaveChangesAsync();

        // Attendees
        var attendees = new[]
        {
            new Attendee { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "512-555-1001", Organization = "Tech Corp", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "512-555-1002", Organization = "StartupXYZ", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "512-555-1003", Organization = "Design Co", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Phone = "512-555-1004", Organization = "University of Austin", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@example.com", Phone = "512-555-1005", Organization = "Health Plus", DietaryNeeds = "Vegetarian", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Frank", LastName = "Miller", Email = "frank.miller@example.com", Phone = "512-555-1006", Organization = "Finance Group", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Grace", LastName = "Wilson", Email = "grace.wilson@example.com", Phone = "512-555-1007", Organization = "Creative Studio", DietaryNeeds = "Gluten-free", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Henry", LastName = "Taylor", Email = "henry.taylor@example.com", Phone = "512-555-1008", Organization = "Tech Corp", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Irene", LastName = "Anderson", Email = "irene.anderson@example.com", Phone = "512-555-1009", Organization = "Wellness Center", DietaryNeeds = "Vegan", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Jack", LastName = "Thomas", Email = "jack.thomas@example.com", Phone = "512-555-1010", Organization = "University of Austin", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Karen", LastName = "Martinez", Email = "karen.martinez@example.com", Phone = "512-555-1011", Organization = "Art Gallery", CreatedAt = now, UpdatedAt = now },
            new Attendee { FirstName = "Leo", LastName = "Garcia", Email = "leo.garcia@example.com", Phone = "512-555-1012", Organization = "StartupXYZ", CreatedAt = now, UpdatedAt = now },
        };
        db.Attendees.AddRange(attendees);
        await db.SaveChangesAsync();

        // Get ticket types by event for registrations
        var evt1Tickets = ticketTypes.Where(t => t.EventId == events[0].Id).ToList();
        var evt2Tickets = ticketTypes.Where(t => t.EventId == events[1].Id).ToList();
        var evt3Tickets = ticketTypes.Where(t => t.EventId == events[2].Id).ToList();
        var evt4Tickets = ticketTypes.Where(t => t.EventId == events[3].Id).ToList();
        var evt5Tickets = ticketTypes.Where(t => t.EventId == events[4].Id).ToList();

        var registrations = new List<Registration>();

        string ConfNum(Event e, int n) => $"SPK-{e.StartDate:yyyyMMdd}-{n:D4}";

        // Event 1 (Austin Tech Summit) - 5 confirmed
        for (int i = 0; i < 5; i++)
        {
            registrations.Add(new Registration
            {
                EventId = events[0].Id, AttendeeId = attendees[i].Id, TicketTypeId = evt1Tickets[i % 3].Id,
                ConfirmationNumber = ConfNum(events[0], i + 1), Status = RegistrationStatus.Confirmed,
                AmountPaid = evt1Tickets[i % 3].Price, RegistrationDate = now.AddDays(-10 + i), CreatedAt = now, UpdatedAt = now
            });
        }
        // Update counts
        events[0].CurrentRegistrations = 5;
        evt1Tickets[0].QuantitySold = 2; evt1Tickets[1].QuantitySold = 2; evt1Tickets[2].QuantitySold = 1;

        // Event 2 (Startup Pitch Night) - 3 confirmed, 1 cancelled
        for (int i = 0; i < 3; i++)
        {
            registrations.Add(new Registration
            {
                EventId = events[1].Id, AttendeeId = attendees[i + 3].Id, TicketTypeId = evt2Tickets[0].Id,
                ConfirmationNumber = ConfNum(events[1], i + 1), Status = RegistrationStatus.Confirmed,
                AmountPaid = evt2Tickets[0].EarlyBirdPrice ?? evt2Tickets[0].Price, RegistrationDate = now.AddDays(-8 + i), CreatedAt = now, UpdatedAt = now
            });
        }
        registrations.Add(new Registration
        {
            EventId = events[1].Id, AttendeeId = attendees[6].Id, TicketTypeId = evt2Tickets[1].Id,
            ConfirmationNumber = ConfNum(events[1], 4), Status = RegistrationStatus.Cancelled,
            AmountPaid = evt2Tickets[1].Price, RegistrationDate = now.AddDays(-7),
            CancellationDate = now.AddDays(-5), CancellationReason = "Schedule conflict",
            CreatedAt = now, UpdatedAt = now
        });
        events[1].CurrentRegistrations = 3;
        evt2Tickets[0].QuantitySold = 3;

        // Event 3 (Watercolor Workshop) - sold out, fill with registrations
        // We need 20 confirmed registrations. We'll re-use some attendees across events.
        for (int i = 0; i < 12; i++)
        {
            var attendeeIdx = i % attendees.Length;
            // Skip if already registered for event 3
            if (registrations.Any(r => r.EventId == events[2].Id && r.AttendeeId == attendees[attendeeIdx].Id)) continue;
            if (registrations.Count(r => r.EventId == events[2].Id && r.Status == RegistrationStatus.Confirmed) >= 20) break;
            registrations.Add(new Registration
            {
                EventId = events[2].Id, AttendeeId = attendees[attendeeIdx].Id, TicketTypeId = evt3Tickets[0].Id,
                ConfirmationNumber = ConfNum(events[2], i + 1), Status = RegistrationStatus.Confirmed,
                AmountPaid = evt3Tickets[0].Price, RegistrationDate = now.AddDays(-15 + i), CreatedAt = now, UpdatedAt = now
            });
        }
        // We may not have 20 unique attendees, but that's fine for demo. Set counts to match entity values.
        var evt3ConfirmedCount = registrations.Count(r => r.EventId == events[2].Id && r.Status == RegistrationStatus.Confirmed);
        events[2].CurrentRegistrations = evt3ConfirmedCount;
        events[2].TotalCapacity = evt3ConfirmedCount; // Adjust so it's "sold out"
        evt3Tickets[0].Quantity = evt3ConfirmedCount;
        evt3Tickets[0].QuantitySold = evt3ConfirmedCount;

        // 3 waitlisted for event 3
        // Use attendees not yet registered for event 3
        var usedForEvt3 = registrations.Where(r => r.EventId == events[2].Id).Select(r => r.AttendeeId).ToHashSet();
        var availableForWaitlist = attendees.Where(a => !usedForEvt3.Contains(a.Id)).Take(3).ToList();
        for (int i = 0; i < availableForWaitlist.Count; i++)
        {
            registrations.Add(new Registration
            {
                EventId = events[2].Id, AttendeeId = availableForWaitlist[i].Id, TicketTypeId = evt3Tickets[0].Id,
                ConfirmationNumber = ConfNum(events[2], evt3ConfirmedCount + i + 1), Status = RegistrationStatus.Waitlisted,
                WaitlistPosition = i + 1, AmountPaid = evt3Tickets[0].Price,
                RegistrationDate = now.AddDays(-3 + i), CreatedAt = now, UpdatedAt = now
            });
        }
        events[2].WaitlistCount = availableForWaitlist.Count;

        // Event 4 (Morning Yoga - today) - 8 confirmed, some checked in
        for (int i = 0; i < 8; i++)
        {
            var status = i < 5 ? RegistrationStatus.CheckedIn : RegistrationStatus.Confirmed;
            registrations.Add(new Registration
            {
                EventId = events[3].Id, AttendeeId = attendees[i].Id, TicketTypeId = evt4Tickets[i % 3].Id,
                ConfirmationNumber = ConfNum(events[3], i + 1), Status = status,
                AmountPaid = evt4Tickets[i % 3].Price, RegistrationDate = now.AddDays(-10 + i),
                CheckInTime = status == RegistrationStatus.CheckedIn ? now.AddHours(-1 + i * 0.1) : null,
                CreatedAt = now, UpdatedAt = now
            });
        }
        evt4Tickets[0].QuantitySold = 3; evt4Tickets[1].QuantitySold = 3; evt4Tickets[2].QuantitySold = 2;

        // Event 5 (AI Conference - completed) - some registrations
        for (int i = 0; i < 5; i++)
        {
            registrations.Add(new Registration
            {
                EventId = events[4].Id, AttendeeId = attendees[i + 2].Id, TicketTypeId = evt5Tickets[0].Id,
                ConfirmationNumber = ConfNum(events[4], i + 1), Status = RegistrationStatus.CheckedIn,
                AmountPaid = evt5Tickets[0].EarlyBirdPrice ?? evt5Tickets[0].Price,
                RegistrationDate = now.AddDays(-30 + i),
                CheckInTime = events[4].StartDate.AddMinutes(10 + i * 5),
                CreatedAt = now, UpdatedAt = now
            });
        }
        evt5Tickets[0].QuantitySold = 5;

        db.Registrations.AddRange(registrations);
        await db.SaveChangesAsync();

        // Check-ins for checked-in registrations
        var checkedInRegs = registrations.Where(r => r.Status == RegistrationStatus.CheckedIn).ToList();
        var checkIns = checkedInRegs.Select(r => new CheckIn
        {
            RegistrationId = r.Id,
            CheckInTime = r.CheckInTime ?? now,
            CheckedInBy = "Staff Admin",
            Notes = null
        }).ToList();
        db.CheckIns.AddRange(checkIns);
        await db.SaveChangesAsync();
    }
}
