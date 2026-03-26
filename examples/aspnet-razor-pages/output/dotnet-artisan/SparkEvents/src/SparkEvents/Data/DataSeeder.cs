using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SparkEventsDbContext db)
    {
        if (await db.EventCategories.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Categories
        var categories = new[]
        {
            new EventCategory { Name = "Technology", Description = "Tech conferences, hackathons, and coding workshops", ColorHex = "#2196F3" },
            new EventCategory { Name = "Business", Description = "Networking events, seminars, and entrepreneurship meetups", ColorHex = "#4CAF50" },
            new EventCategory { Name = "Creative Arts", Description = "Art exhibitions, music events, and creative workshops", ColorHex = "#FF9800" },
            new EventCategory { Name = "Health & Wellness", Description = "Fitness events, wellness retreats, and health seminars", ColorHex = "#E91E63" },
        };
        db.EventCategories.AddRange(categories);
        await db.SaveChangesAsync();

        // Venues
        var venues = new[]
        {
            new Venue
            {
                Name = "Innovation Hub", Address = "123 Tech Drive", City = "Austin", State = "TX", ZipCode = "78701",
                MaxCapacity = 200, ContactEmail = "info@innovationhub.com", ContactPhone = "512-555-0100",
                Notes = "Free parking available. AV equipment included."
            },
            new Venue
            {
                Name = "Grand Convention Center", Address = "500 Main Street", City = "Denver", State = "CO", ZipCode = "80202",
                MaxCapacity = 500, ContactEmail = "events@grandcc.com", ContactPhone = "303-555-0200",
                Notes = "Multiple halls and breakout rooms available."
            },
            new Venue
            {
                Name = "Community Workshop Space", Address = "42 Oak Lane", City = "Portland", State = "OR", ZipCode = "97201",
                MaxCapacity = 50, ContactEmail = "hello@communityws.com", ContactPhone = "503-555-0300",
                Notes = "Intimate setting, perfect for workshops."
            },
        };
        db.Venues.AddRange(venues);
        await db.SaveChangesAsync();

        // Events
        var today = now.Date;

        var events = new List<Event>
        {
            // Event 1: Upcoming Published — future event with capacity
            new()
            {
                Title = "DevCon 2026: Building the Future",
                Description = "A premier developer conference featuring talks on AI, cloud-native development, and emerging technologies. Join industry leaders for two days of learning and networking.",
                EventCategoryId = categories[0].Id, VenueId = venues[1].Id,
                StartDate = today.AddDays(30), EndDate = today.AddDays(31),
                RegistrationOpenDate = today.AddDays(-10), RegistrationCloseDate = today.AddDays(28),
                EarlyBirdDeadline = today.AddDays(-3),
                TotalCapacity = 300, Status = EventStatus.Published, IsFeatured = true,
            },
            // Event 2: Upcoming Published
            new()
            {
                Title = "Startup Pitch Night",
                Description = "An evening of exciting startup pitches where entrepreneurs present their ideas to a panel of investors and mentors. Network with fellow founders and industry experts.",
                EventCategoryId = categories[1].Id, VenueId = venues[0].Id,
                StartDate = today.AddDays(14), EndDate = today.AddDays(14).AddHours(4),
                RegistrationOpenDate = today.AddDays(-7), RegistrationCloseDate = today.AddDays(12),
                EarlyBirdDeadline = today.AddDays(-1),
                TotalCapacity = 100, Status = EventStatus.Published, IsFeatured = false,
            },
            // Event 3: SoldOut with waitlist
            new()
            {
                Title = "Mindful Movement Workshop",
                Description = "A hands-on wellness workshop combining yoga, meditation, and breathwork techniques. Learn practical mindfulness skills you can apply in your daily life.",
                EventCategoryId = categories[3].Id, VenueId = venues[2].Id,
                StartDate = today.AddDays(7), EndDate = today.AddDays(7).AddHours(3),
                RegistrationOpenDate = today.AddDays(-14), RegistrationCloseDate = today.AddDays(5),
                TotalCapacity = 30, Status = EventStatus.SoldOut, IsFeatured = true,
            },
            // Event 4: Today/Tomorrow — for check-in testing
            new()
            {
                Title = "Creative Coding Lab",
                Description = "An interactive workshop exploring the intersection of art and technology. Create generative art, interactive installations, and creative visualizations using code.",
                EventCategoryId = categories[2].Id, VenueId = venues[2].Id,
                StartDate = today.AddHours(9), EndDate = today.AddHours(17),
                RegistrationOpenDate = today.AddDays(-21), RegistrationCloseDate = today.AddDays(-1),
                TotalCapacity = 40, Status = EventStatus.Published, IsFeatured = false,
            },
            // Event 5: Completed
            new()
            {
                Title = "Cloud Architecture Summit 2025",
                Description = "A comprehensive conference on cloud architecture patterns, microservices, and distributed systems. Attended by 150+ professionals.",
                EventCategoryId = categories[0].Id, VenueId = venues[1].Id,
                StartDate = today.AddDays(-30), EndDate = today.AddDays(-29),
                RegistrationOpenDate = today.AddDays(-60), RegistrationCloseDate = today.AddDays(-31),
                EarlyBirdDeadline = today.AddDays(-45),
                TotalCapacity = 200, Status = EventStatus.Completed, IsFeatured = false,
            },
            // Event 6: Draft
            new()
            {
                Title = "Entrepreneurship Bootcamp",
                Description = "A week-long intensive bootcamp for aspiring entrepreneurs. Topics include business planning, funding strategies, marketing, and MVP development.",
                EventCategoryId = categories[1].Id, VenueId = venues[0].Id,
                StartDate = today.AddDays(60), EndDate = today.AddDays(64),
                RegistrationOpenDate = today.AddDays(30), RegistrationCloseDate = today.AddDays(58),
                EarlyBirdDeadline = today.AddDays(45),
                TotalCapacity = 50, Status = EventStatus.Draft, IsFeatured = false,
            },
        };

        db.Events.AddRange(events);
        await db.SaveChangesAsync();

        // Ticket Types (3 per event)
        var ticketTypes = new List<TicketType>();
        foreach (var evt in events)
        {
            ticketTypes.AddRange(
            [
                new TicketType { EventId = evt.Id, Name = "General Admission", Price = 25.00m, EarlyBirdPrice = 15.00m, Quantity = (int)(evt.TotalCapacity * 0.6), SortOrder = 1 },
                new TicketType { EventId = evt.Id, Name = "VIP", Description = "Includes front-row seating and exclusive networking session", Price = 75.00m, EarlyBirdPrice = 55.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 2 },
                new TicketType { EventId = evt.Id, Name = "Student", Description = "Valid student ID required at check-in", Price = 10.00m, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 3 },
            ]);
        }
        db.TicketTypes.AddRange(ticketTypes);
        await db.SaveChangesAsync();

        // Attendees
        var attendees = new[]
        {
            new Attendee { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0101", Organization = "TechCorp" },
            new Attendee { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "555-0102", Organization = "StartupXYZ" },
            new Attendee { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Organization = "DesignStudio" },
            new Attendee { FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Phone = "555-0104", DietaryNeeds = "Vegetarian" },
            new Attendee { FirstName = "Eva", LastName = "Martinez", Email = "eva.martinez@example.com", Organization = "CloudInc", DietaryNeeds = "Gluten-free" },
            new Attendee { FirstName = "Frank", LastName = "Taylor", Email = "frank.taylor@example.com", Phone = "555-0106" },
            new Attendee { FirstName = "Grace", LastName = "Lee", Email = "grace.lee@example.com", Organization = "University of Tech" },
            new Attendee { FirstName = "Henry", LastName = "Wilson", Email = "henry.wilson@example.com", Phone = "555-0108", Organization = "MegaCorp" },
            new Attendee { FirstName = "Ivy", LastName = "Chen", Email = "ivy.chen@example.com", DietaryNeeds = "Vegan" },
            new Attendee { FirstName = "Jack", LastName = "Davis", Email = "jack.davis@example.com", Phone = "555-0110", Organization = "FreelanceDev" },
            new Attendee { FirstName = "Karen", LastName = "Moore", Email = "karen.moore@example.com", Organization = "HealthFirst" },
            new Attendee { FirstName = "Leo", LastName = "Garcia", Email = "leo.garcia@example.com", Phone = "555-0112" },
        };
        db.Attendees.AddRange(attendees);
        await db.SaveChangesAsync();

        // Helper to generate confirmation numbers
        var regCounters = new Dictionary<int, int>();
        string NextConfirmation(Event evt)
        {
            if (!regCounters.TryGetValue(evt.Id, out var count))
            {
                count = 0;
            }
            count++;
            regCounters[evt.Id] = count;
            return $"SPK-{evt.StartDate:yyyyMMdd}-{count:D4}";
        }

        // Get ticket types by event
        var ttByEvent = ticketTypes.GroupBy(t => t.EventId).ToDictionary(g => g.Key, g => g.ToList());

        var registrations = new List<Registration>();

        // Event 1 (DevCon) — several confirmed registrations (early-bird deadline passed)
        var devcon = events[0];
        var devconTT = ttByEvent[devcon.Id];
        registrations.AddRange(
        [
            new Registration { EventId = devcon.Id, AttendeeId = attendees[0].Id, TicketTypeId = devconTT[0].Id, ConfirmationNumber = NextConfirmation(devcon), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = today.AddDays(-8) },
            new Registration { EventId = devcon.Id, AttendeeId = attendees[1].Id, TicketTypeId = devconTT[1].Id, ConfirmationNumber = NextConfirmation(devcon), Status = RegistrationStatus.Confirmed, AmountPaid = 55.00m, RegistrationDate = today.AddDays(-7) },
            new Registration { EventId = devcon.Id, AttendeeId = attendees[2].Id, TicketTypeId = devconTT[0].Id, ConfirmationNumber = NextConfirmation(devcon), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = today.AddDays(-1) },
            new Registration { EventId = devcon.Id, AttendeeId = attendees[3].Id, TicketTypeId = devconTT[2].Id, ConfirmationNumber = NextConfirmation(devcon), Status = RegistrationStatus.Confirmed, AmountPaid = 10.00m, RegistrationDate = today.AddDays(-5) },
            new Registration { EventId = devcon.Id, AttendeeId = attendees[7].Id, TicketTypeId = devconTT[0].Id, ConfirmationNumber = NextConfirmation(devcon), Status = RegistrationStatus.Cancelled, AmountPaid = 15.00m, RegistrationDate = today.AddDays(-9), CancellationDate = today.AddDays(-6), CancellationReason = "Schedule conflict" },
        ]);

        // Event 2 (Startup Pitch) — some registrations
        var pitch = events[1];
        var pitchTT = ttByEvent[pitch.Id];
        registrations.AddRange(
        [
            new Registration { EventId = pitch.Id, AttendeeId = attendees[4].Id, TicketTypeId = pitchTT[0].Id, ConfirmationNumber = NextConfirmation(pitch), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = today.AddDays(-5) },
            new Registration { EventId = pitch.Id, AttendeeId = attendees[5].Id, TicketTypeId = pitchTT[1].Id, ConfirmationNumber = NextConfirmation(pitch), Status = RegistrationStatus.Confirmed, AmountPaid = 55.00m, RegistrationDate = today.AddDays(-4) },
            new Registration { EventId = pitch.Id, AttendeeId = attendees[6].Id, TicketTypeId = pitchTT[2].Id, ConfirmationNumber = NextConfirmation(pitch), Status = RegistrationStatus.Confirmed, AmountPaid = 10.00m, RegistrationDate = today.AddDays(-3) },
        ]);

        // Event 3 (Mindful Movement — SoldOut) — fill to capacity + waitlist
        var wellness = events[2];
        var wellnessTT = ttByEvent[wellness.Id];
        for (var i = 0; i < 10; i++)
        {
            var att = attendees[i % attendees.Length];
            // Skip if same attendee+event combo already exists
            if (registrations.Any(r => r.EventId == wellness.Id && r.AttendeeId == att.Id))
            {
                continue;
            }
            registrations.Add(new Registration
            {
                EventId = wellness.Id, AttendeeId = att.Id, TicketTypeId = wellnessTT[0].Id,
                ConfirmationNumber = NextConfirmation(wellness), Status = RegistrationStatus.Confirmed,
                AmountPaid = 25.00m, RegistrationDate = today.AddDays(-12 + i),
            });
        }
        // Waitlisted
        registrations.Add(new Registration
        {
            EventId = wellness.Id, AttendeeId = attendees[10].Id, TicketTypeId = wellnessTT[0].Id,
            ConfirmationNumber = NextConfirmation(wellness), Status = RegistrationStatus.Waitlisted,
            AmountPaid = 0m, WaitlistPosition = 1, RegistrationDate = today.AddDays(-2),
        });
        registrations.Add(new Registration
        {
            EventId = wellness.Id, AttendeeId = attendees[11].Id, TicketTypeId = wellnessTT[0].Id,
            ConfirmationNumber = NextConfirmation(wellness), Status = RegistrationStatus.Waitlisted,
            AmountPaid = 0m, WaitlistPosition = 2, RegistrationDate = today.AddDays(-1),
        });

        // Event 4 (Creative Coding Lab — today) — some confirmed, some checked in
        var creative = events[3];
        var creativeTT = ttByEvent[creative.Id];
        registrations.AddRange(
        [
            new Registration { EventId = creative.Id, AttendeeId = attendees[0].Id, TicketTypeId = creativeTT[0].Id, ConfirmationNumber = NextConfirmation(creative), Status = RegistrationStatus.CheckedIn, AmountPaid = 25.00m, RegistrationDate = today.AddDays(-14), CheckInTime = today.AddHours(8.5) },
            new Registration { EventId = creative.Id, AttendeeId = attendees[2].Id, TicketTypeId = creativeTT[1].Id, ConfirmationNumber = NextConfirmation(creative), Status = RegistrationStatus.CheckedIn, AmountPaid = 75.00m, RegistrationDate = today.AddDays(-10), CheckInTime = today.AddHours(8.75) },
            new Registration { EventId = creative.Id, AttendeeId = attendees[4].Id, TicketTypeId = creativeTT[2].Id, ConfirmationNumber = NextConfirmation(creative), Status = RegistrationStatus.CheckedIn, AmountPaid = 10.00m, RegistrationDate = today.AddDays(-12), CheckInTime = today.AddHours(9) },
            new Registration { EventId = creative.Id, AttendeeId = attendees[6].Id, TicketTypeId = creativeTT[0].Id, ConfirmationNumber = NextConfirmation(creative), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = today.AddDays(-8) },
            new Registration { EventId = creative.Id, AttendeeId = attendees[8].Id, TicketTypeId = creativeTT[0].Id, ConfirmationNumber = NextConfirmation(creative), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = today.AddDays(-7) },
        ]);

        // Event 5 (Cloud Architecture — Completed) — checked in + no-show
        var cloud = events[4];
        var cloudTT = ttByEvent[cloud.Id];
        registrations.AddRange(
        [
            new Registration { EventId = cloud.Id, AttendeeId = attendees[1].Id, TicketTypeId = cloudTT[0].Id, ConfirmationNumber = NextConfirmation(cloud), Status = RegistrationStatus.CheckedIn, AmountPaid = 15.00m, RegistrationDate = today.AddDays(-55), CheckInTime = today.AddDays(-30).AddHours(8) },
            new Registration { EventId = cloud.Id, AttendeeId = attendees[3].Id, TicketTypeId = cloudTT[1].Id, ConfirmationNumber = NextConfirmation(cloud), Status = RegistrationStatus.CheckedIn, AmountPaid = 55.00m, RegistrationDate = today.AddDays(-50), CheckInTime = today.AddDays(-30).AddHours(8.5) },
            new Registration { EventId = cloud.Id, AttendeeId = attendees[5].Id, TicketTypeId = cloudTT[0].Id, ConfirmationNumber = NextConfirmation(cloud), Status = RegistrationStatus.NoShow, AmountPaid = 25.00m, RegistrationDate = today.AddDays(-40) },
            new Registration { EventId = cloud.Id, AttendeeId = attendees[9].Id, TicketTypeId = cloudTT[2].Id, ConfirmationNumber = NextConfirmation(cloud), Status = RegistrationStatus.CheckedIn, AmountPaid = 10.00m, RegistrationDate = today.AddDays(-48), CheckInTime = today.AddDays(-30).AddHours(9) },
        ]);

        db.Registrations.AddRange(registrations);
        await db.SaveChangesAsync();

        // Update counts on events and ticket types
        foreach (var evt in events)
        {
            var evtRegs = registrations.Where(r => r.EventId == evt.Id).ToList();
            evt.CurrentRegistrations = evtRegs.Count(r => r.Status is RegistrationStatus.Confirmed or RegistrationStatus.CheckedIn);
            evt.WaitlistCount = evtRegs.Count(r => r.Status == RegistrationStatus.Waitlisted);
        }

        foreach (var tt in ticketTypes)
        {
            tt.QuantitySold = registrations.Count(r => r.TicketTypeId == tt.Id && r.Status is RegistrationStatus.Confirmed or RegistrationStatus.CheckedIn);
        }

        await db.SaveChangesAsync();

        // Check-ins for checked-in registrations
        var checkedInRegs = registrations.Where(r => r.Status == RegistrationStatus.CheckedIn).ToList();
        var checkIns = checkedInRegs.Select(r => new CheckIn
        {
            RegistrationId = r.Id,
            CheckInTime = r.CheckInTime ?? now,
            CheckedInBy = "Staff Member",
            Notes = null,
        }).ToList();

        db.CheckIns.AddRange(checkIns);
        await db.SaveChangesAsync();
    }
}
