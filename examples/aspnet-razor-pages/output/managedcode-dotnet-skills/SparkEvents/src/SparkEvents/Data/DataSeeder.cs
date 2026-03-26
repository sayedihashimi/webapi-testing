using SparkEvents.Models;

namespace SparkEvents.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SparkEventsDbContext context)
    {
        if (context.EventCategories.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Categories
        var technology = new EventCategory { Name = "Technology", Description = "Tech conferences, hackathons, and coding workshops", ColorHex = "#0D6EFD" };
        var business = new EventCategory { Name = "Business", Description = "Business networking, entrepreneurship, and leadership events", ColorHex = "#198754" };
        var creativeArts = new EventCategory { Name = "Creative Arts", Description = "Art exhibitions, music performances, and creative workshops", ColorHex = "#6F42C1" };
        var healthWellness = new EventCategory { Name = "Health & Wellness", Description = "Fitness classes, mindfulness sessions, and health seminars", ColorHex = "#DC3545" };

        context.EventCategories.AddRange(technology, business, creativeArts, healthWellness);

        // Venues
        var grandHall = new Venue
        {
            Name = "Grand Convention Center",
            Address = "1200 Exhibition Blvd",
            City = "Austin",
            State = "TX",
            ZipCode = "78701",
            MaxCapacity = 500,
            ContactEmail = "events@grandconvention.example.com",
            ContactPhone = "(512) 555-0100",
            Notes = "Full AV setup, multiple breakout rooms available",
            CreatedAt = now
        };
        var innovationHub = new Venue
        {
            Name = "Innovation Hub",
            Address = "450 Startup Lane",
            City = "Austin",
            State = "TX",
            ZipCode = "78702",
            MaxCapacity = 200,
            ContactEmail = "hello@innovationhub.example.com",
            ContactPhone = "(512) 555-0200",
            Notes = "Modern co-working space with presentation area",
            CreatedAt = now
        };
        var communityCenter = new Venue
        {
            Name = "Riverside Community Center",
            Address = "88 River Road",
            City = "Austin",
            State = "TX",
            ZipCode = "78704",
            MaxCapacity = 50,
            ContactEmail = "info@riversidecc.example.com",
            ContactPhone = "(512) 555-0300",
            CreatedAt = now
        };

        context.Venues.AddRange(grandHall, innovationHub, communityCenter);
        await context.SaveChangesAsync();

        // Events
        // 1. Published tech conference (upcoming, 2 weeks from now, featured)
        var techConference = new Event
        {
            Title = "Austin Tech Summit 2026",
            Description = "The premier technology conference in Central Texas featuring keynote speakers from leading tech companies, hands-on workshops, and networking opportunities. Topics include AI, cloud computing, cybersecurity, and emerging technologies.",
            EventCategoryId = technology.Id,
            VenueId = grandHall.Id,
            StartDate = now.AddDays(14).Date.AddHours(9),
            EndDate = now.AddDays(14).Date.AddHours(18),
            RegistrationOpenDate = now.AddDays(-30),
            RegistrationCloseDate = now.AddDays(13),
            EarlyBirdDeadline = now.AddDays(-7),
            TotalCapacity = 200,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = true,
            CreatedAt = now.AddDays(-35),
            UpdatedAt = now.AddDays(-30)
        };

        // 2. Published workshop (upcoming, 1 week from now)
        var bizWorkshop = new Event
        {
            Title = "Leadership Masterclass",
            Description = "An intensive full-day workshop on modern leadership principles, team management, and strategic thinking. Perfect for managers and aspiring leaders looking to level up their skills.",
            EventCategoryId = business.Id,
            VenueId = innovationHub.Id,
            StartDate = now.AddDays(7).Date.AddHours(10),
            EndDate = now.AddDays(7).Date.AddHours(16),
            RegistrationOpenDate = now.AddDays(-21),
            RegistrationCloseDate = now.AddDays(6),
            EarlyBirdDeadline = now.AddDays(-3),
            TotalCapacity = 50,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-25),
            UpdatedAt = now.AddDays(-21)
        };

        // 3. SoldOut event (upcoming, 3 days from now)
        var artExhibition = new Event
        {
            Title = "Creative Coding Art Show",
            Description = "An exclusive exhibition showcasing digital art created through programming. Features live coding demonstrations, interactive installations, and a gallery of generative art pieces.",
            EventCategoryId = creativeArts.Id,
            VenueId = communityCenter.Id,
            StartDate = now.AddDays(3).Date.AddHours(18),
            EndDate = now.AddDays(3).Date.AddHours(22),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.AddDays(2),
            TotalCapacity = 30,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.SoldOut,
            IsFeatured = true,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-5)
        };

        // 4. Event today/tomorrow (for check-in testing)
        var todayEvent = new Event
        {
            Title = "Morning Yoga & Mindfulness",
            Description = "Start your day with an energizing yoga session followed by guided meditation and mindfulness exercises. Suitable for all levels from beginners to experienced practitioners.",
            EventCategoryId = healthWellness.Id,
            VenueId = communityCenter.Id,
            StartDate = now.Date.AddDays(1).AddHours(7),
            EndDate = now.Date.AddDays(1).AddHours(10),
            RegistrationOpenDate = now.AddDays(-10),
            RegistrationCloseDate = now.Date.AddDays(1),
            TotalCapacity = 30,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-14),
            UpdatedAt = now.AddDays(-10)
        };

        // 5. Completed event (past, 2 weeks ago)
        var completedEvent = new Event
        {
            Title = "Startup Pitch Night",
            Description = "An exciting evening where local entrepreneurs pitched their startup ideas to a panel of investors and mentors. Featuring networking reception and awards ceremony.",
            EventCategoryId = business.Id,
            VenueId = innovationHub.Id,
            StartDate = now.AddDays(-14).Date.AddHours(18),
            EndDate = now.AddDays(-14).Date.AddHours(22),
            RegistrationOpenDate = now.AddDays(-45),
            RegistrationCloseDate = now.AddDays(-15),
            EarlyBirdDeadline = now.AddDays(-30),
            TotalCapacity = 100,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Completed,
            IsFeatured = false,
            CreatedAt = now.AddDays(-50),
            UpdatedAt = now.AddDays(-14)
        };

        // 6. Draft event (future, 1 month from now)
        var draftEvent = new Event
        {
            Title = "AI & Machine Learning Bootcamp",
            Description = "A 2-day intensive bootcamp covering the fundamentals of artificial intelligence and machine learning. Participants will build real ML models using Python and popular frameworks.",
            EventCategoryId = technology.Id,
            VenueId = grandHall.Id,
            StartDate = now.AddDays(30).Date.AddHours(9),
            EndDate = now.AddDays(31).Date.AddHours(17),
            RegistrationOpenDate = now.AddDays(7),
            RegistrationCloseDate = now.AddDays(29),
            EarlyBirdDeadline = now.AddDays(20),
            TotalCapacity = 150,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Draft,
            IsFeatured = false,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3)
        };

        context.Events.AddRange(techConference, bizWorkshop, artExhibition, todayEvent, completedEvent, draftEvent);
        await context.SaveChangesAsync();

        // Ticket Types (3 per event)
        var ticketTypes = new List<TicketType>();

        void AddTicketTypes(Event evt, decimal gaPrice, decimal vipPrice, decimal studentPrice, decimal? gaEarly = null, decimal? vipEarly = null)
        {
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "General Admission", Description = "Standard event access", Price = gaPrice, EarlyBirdPrice = gaEarly, Quantity = (int)(evt.TotalCapacity * 0.6), SortOrder = 1, IsActive = true, CreatedAt = evt.CreatedAt });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "VIP", Description = "Premium access with reserved seating and exclusive perks", Price = vipPrice, EarlyBirdPrice = vipEarly, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 2, IsActive = true, CreatedAt = evt.CreatedAt });
            ticketTypes.Add(new TicketType { EventId = evt.Id, Name = "Student", Description = "Discounted rate for students with valid ID", Price = studentPrice, Quantity = (int)(evt.TotalCapacity * 0.2), SortOrder = 3, IsActive = true, CreatedAt = evt.CreatedAt });
        }

        AddTicketTypes(techConference, 45.00m, 125.00m, 15.00m, 35.00m, 95.00m);
        AddTicketTypes(bizWorkshop, 35.00m, 85.00m, 12.00m, 25.00m);
        AddTicketTypes(artExhibition, 20.00m, 50.00m, 8.00m);
        AddTicketTypes(todayEvent, 15.00m, 30.00m, 5.00m);
        AddTicketTypes(completedEvent, 25.00m, 75.00m, 10.00m, 18.00m, 55.00m);
        AddTicketTypes(draftEvent, 60.00m, 150.00m, 25.00m, 45.00m, 110.00m);

        context.TicketTypes.AddRange(ticketTypes);
        await context.SaveChangesAsync();

        // Build ticket type lookups per event
        var techGA = ticketTypes[0]; var techVIP = ticketTypes[1]; var techStudent = ticketTypes[2];
        var bizGA = ticketTypes[3]; var bizVIP = ticketTypes[4]; var bizStudent = ticketTypes[5];
        var artGA = ticketTypes[6]; var artVIP = ticketTypes[7]; var artStudent = ticketTypes[8];
        var yogaGA = ticketTypes[9]; var yogaVIP = ticketTypes[10]; var yogaStudent = ticketTypes[11];
        var pitchGA = ticketTypes[12]; var pitchVIP = ticketTypes[13]; var pitchStudent = ticketTypes[14];

        // Attendees (12)
        var attendees = new List<Attendee>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "(512) 555-1001", Organization = "TechCorp Inc.", CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-40) },
            new() { FirstName = "Bob", LastName = "Martinez", Email = "bob.martinez@example.com", Phone = "(512) 555-1002", Organization = "StartupHub", CreatedAt = now.AddDays(-38), UpdatedAt = now.AddDays(-38) },
            new() { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "(512) 555-1003", Organization = "DesignStudio", DietaryNeeds = "Vegetarian", CreatedAt = now.AddDays(-36), UpdatedAt = now.AddDays(-36) },
            new() { FirstName = "David", LastName = "Chen", Email = "david.chen@example.com", Phone = "(512) 555-1004", Organization = "DataDriven LLC", CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-35) },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@example.com", Organization = "University of Texas", DietaryNeeds = "Gluten-free", CreatedAt = now.AddDays(-34), UpdatedAt = now.AddDays(-34) },
            new() { FirstName = "Frank", LastName = "O'Brien", Email = "frank.obrien@example.com", Phone = "(512) 555-1006", Organization = "CloudNine Systems", CreatedAt = now.AddDays(-33), UpdatedAt = now.AddDays(-33) },
            new() { FirstName = "Grace", LastName = "Kim", Email = "grace.kim@example.com", Phone = "(512) 555-1007", Organization = "ArtTech Collective", CreatedAt = now.AddDays(-32), UpdatedAt = now.AddDays(-32) },
            new() { FirstName = "Henry", LastName = "Patel", Email = "henry.patel@example.com", Organization = "Wellness Works", DietaryNeeds = "Vegan", CreatedAt = now.AddDays(-31), UpdatedAt = now.AddDays(-31) },
            new() { FirstName = "Isabelle", LastName = "Thompson", Email = "isabelle.thompson@example.com", Phone = "(512) 555-1009", Organization = "FinanceFirst", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-30) },
            new() { FirstName = "James", LastName = "Wilson", Email = "james.wilson@example.com", Phone = "(512) 555-1010", Organization = "GameDev Studio", CreatedAt = now.AddDays(-29), UpdatedAt = now.AddDays(-29) },
            new() { FirstName = "Karen", LastName = "Lee", Email = "karen.lee@example.com", Phone = "(512) 555-1011", Organization = "MediaWorks", CreatedAt = now.AddDays(-28), UpdatedAt = now.AddDays(-28) },
            new() { FirstName = "Leo", LastName = "Santos", Email = "leo.santos@example.com", Organization = "University of Texas", CreatedAt = now.AddDays(-27), UpdatedAt = now.AddDays(-27) }
        };

        context.Attendees.AddRange(attendees);
        await context.SaveChangesAsync();

        var registrations = new List<Registration>();
        int regCounter = 1;

        string ConfNum(Event evt, int num) =>
            $"SPK-{evt.StartDate:yyyyMMdd}-{num:D4}";

        // --- Tech Conference registrations (5 confirmed, some early-bird) ---
        registrations.Add(new Registration { EventId = techConference.Id, AttendeeId = attendees[0].Id, TicketTypeId = techGA.Id, ConfirmationNumber = ConfNum(techConference, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 35.00m, RegistrationDate = now.AddDays(-28), CreatedAt = now.AddDays(-28), UpdatedAt = now.AddDays(-28) });
        registrations.Add(new Registration { EventId = techConference.Id, AttendeeId = attendees[1].Id, TicketTypeId = techVIP.Id, ConfirmationNumber = ConfNum(techConference, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 95.00m, RegistrationDate = now.AddDays(-25), CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-25) });
        registrations.Add(new Registration { EventId = techConference.Id, AttendeeId = attendees[4].Id, TicketTypeId = techStudent.Id, ConfirmationNumber = ConfNum(techConference, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) });
        registrations.Add(new Registration { EventId = techConference.Id, AttendeeId = attendees[5].Id, TicketTypeId = techGA.Id, ConfirmationNumber = ConfNum(techConference, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 45.00m, RegistrationDate = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3) });
        registrations.Add(new Registration { EventId = techConference.Id, AttendeeId = attendees[9].Id, TicketTypeId = techGA.Id, ConfirmationNumber = ConfNum(techConference, regCounter++), Status = RegistrationStatus.Cancelled, AmountPaid = 35.00m, RegistrationDate = now.AddDays(-20), CancellationDate = now.AddDays(-15), CancellationReason = "Schedule conflict", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-15) });

        techGA.QuantitySold = 2; // Alice + Frank (James cancelled)
        techVIP.QuantitySold = 1; // Bob
        techStudent.QuantitySold = 1; // Emily
        techConference.CurrentRegistrations = 4;

        // --- Business Workshop registrations (3 confirmed) ---
        regCounter = 1;
        registrations.Add(new Registration { EventId = bizWorkshop.Id, AttendeeId = attendees[1].Id, TicketTypeId = bizGA.Id, ConfirmationNumber = ConfNum(bizWorkshop, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-18), CreatedAt = now.AddDays(-18), UpdatedAt = now.AddDays(-18) });
        registrations.Add(new Registration { EventId = bizWorkshop.Id, AttendeeId = attendees[3].Id, TicketTypeId = bizVIP.Id, ConfirmationNumber = ConfNum(bizWorkshop, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 85.00m, RegistrationDate = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) });
        registrations.Add(new Registration { EventId = bizWorkshop.Id, AttendeeId = attendees[8].Id, TicketTypeId = bizStudent.Id, ConfirmationNumber = ConfNum(bizWorkshop, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 12.00m, RegistrationDate = now.AddDays(-4), CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-4) });

        bizGA.QuantitySold = 1;
        bizVIP.QuantitySold = 1;
        bizStudent.QuantitySold = 1;
        bizWorkshop.CurrentRegistrations = 3;

        // --- Art Exhibition registrations (SoldOut: 30 total capacity, fill it + 2 waitlisted) ---
        regCounter = 1;
        // Fill all 30 spots: 18 GA, 6 VIP, 6 Student
        for (int i = 0; i < 10 && i < attendees.Count; i++)
        {
            var tt = i < 6 ? artGA : (i < 8 ? artVIP : artStudent);
            registrations.Add(new Registration { EventId = artExhibition.Id, AttendeeId = attendees[i].Id, TicketTypeId = tt.Id, ConfirmationNumber = ConfNum(artExhibition, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = tt.Price, RegistrationDate = now.AddDays(-12 + i), CreatedAt = now.AddDays(-12 + i), UpdatedAt = now.AddDays(-12 + i) });
        }

        artGA.QuantitySold = 6;
        artVIP.QuantitySold = 2;
        artStudent.QuantitySold = 2;
        artExhibition.CurrentRegistrations = 10;

        // We said capacity is 30 but only 12 attendees. Let's set capacity to 10 to make it truly sold out
        artExhibition.TotalCapacity = 10;
        // Recalculate ticket quantities
        artGA.Quantity = 6;
        artVIP.Quantity = 2;
        artStudent.Quantity = 2;

        // 2 waitlisted
        registrations.Add(new Registration { EventId = artExhibition.Id, AttendeeId = attendees[10].Id, TicketTypeId = artGA.Id, ConfirmationNumber = ConfNum(artExhibition, regCounter++), Status = RegistrationStatus.Waitlisted, AmountPaid = 0m, WaitlistPosition = 1, RegistrationDate = now.AddDays(-2), CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) });
        registrations.Add(new Registration { EventId = artExhibition.Id, AttendeeId = attendees[11].Id, TicketTypeId = artGA.Id, ConfirmationNumber = ConfNum(artExhibition, regCounter++), Status = RegistrationStatus.Waitlisted, AmountPaid = 0m, WaitlistPosition = 2, RegistrationDate = now.AddDays(-1), CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1) });
        artExhibition.WaitlistCount = 2;

        // --- Today/Tomorrow event registrations (6 confirmed, for check-in testing) ---
        regCounter = 1;
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[2].Id, TicketTypeId = yogaGA.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-8), CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8) });
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[6].Id, TicketTypeId = yogaGA.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-7), CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7) });
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[7].Id, TicketTypeId = yogaVIP.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 30.00m, RegistrationDate = now.AddDays(-6), CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-6) });
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[0].Id, TicketTypeId = yogaStudent.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 5.00m, RegistrationDate = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5) });
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[4].Id, TicketTypeId = yogaGA.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-4), CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-4) });
        registrations.Add(new Registration { EventId = todayEvent.Id, AttendeeId = attendees[10].Id, TicketTypeId = yogaGA.Id, ConfirmationNumber = ConfNum(todayEvent, regCounter++), Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3) });

        yogaGA.QuantitySold = 4;
        yogaVIP.QuantitySold = 1;
        yogaStudent.QuantitySold = 1;
        todayEvent.CurrentRegistrations = 6;

        // --- Completed event registrations (6 confirmed/checked-in, 1 cancelled) ---
        regCounter = 1;
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[0].Id, TicketTypeId = pitchGA.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.CheckedIn, AmountPaid = 18.00m, RegistrationDate = now.AddDays(-40), CheckInTime = completedEvent.StartDate.AddMinutes(10), CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[1].Id, TicketTypeId = pitchVIP.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.CheckedIn, AmountPaid = 55.00m, RegistrationDate = now.AddDays(-38), CheckInTime = completedEvent.StartDate.AddMinutes(5), CreatedAt = now.AddDays(-38), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[2].Id, TicketTypeId = pitchGA.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.CheckedIn, AmountPaid = 18.00m, RegistrationDate = now.AddDays(-35), CheckInTime = completedEvent.StartDate.AddMinutes(15), CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[3].Id, TicketTypeId = pitchStudent.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.CheckedIn, AmountPaid = 10.00m, RegistrationDate = now.AddDays(-33), CheckInTime = completedEvent.StartDate.AddMinutes(20), CreatedAt = now.AddDays(-33), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[5].Id, TicketTypeId = pitchGA.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.CheckedIn, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-20), CheckInTime = completedEvent.StartDate.AddMinutes(30), CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[6].Id, TicketTypeId = pitchGA.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.NoShow, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-18), CreatedAt = now.AddDays(-18), UpdatedAt = now.AddDays(-14) });
        registrations.Add(new Registration { EventId = completedEvent.Id, AttendeeId = attendees[7].Id, TicketTypeId = pitchGA.Id, ConfirmationNumber = ConfNum(completedEvent, regCounter++), Status = RegistrationStatus.Cancelled, AmountPaid = 18.00m, RegistrationDate = now.AddDays(-30), CancellationDate = now.AddDays(-20), CancellationReason = "Unable to attend", CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-20) });

        pitchGA.QuantitySold = 4; // 3 checked-in + 1 no-show (cancelled doesn't count toward sold)
        pitchVIP.QuantitySold = 1;
        pitchStudent.QuantitySold = 1;
        completedEvent.CurrentRegistrations = 6; // 5 checked-in + 1 no-show

        context.Registrations.AddRange(registrations);
        await context.SaveChangesAsync();

        // Check-ins for completed event and today's event
        var checkIns = new List<CheckIn>();

        // Find checked-in registrations
        var checkedInRegs = registrations.Where(r => r.Status == RegistrationStatus.CheckedIn).ToList();
        foreach (var reg in checkedInRegs)
        {
            checkIns.Add(new CheckIn
            {
                RegistrationId = reg.Id,
                CheckInTime = reg.CheckInTime ?? completedEvent.StartDate.AddMinutes(10),
                CheckedInBy = "Staff Member",
                Notes = null
            });
        }

        context.CheckIns.AddRange(checkIns);
        await context.SaveChangesAsync();
    }
}
