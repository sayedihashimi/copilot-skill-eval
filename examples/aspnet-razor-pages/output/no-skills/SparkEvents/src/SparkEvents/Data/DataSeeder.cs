using SparkEvents.Models;

namespace SparkEvents.Data;

public static class DataSeeder
{
    public static void Seed(SparkEventsDbContext db)
    {
        if (db.EventCategories.Any()) return;

        // Categories
        var categories = new List<EventCategory>
        {
            new() { Name = "Technology", Description = "Tech conferences, hackathons, and coding workshops", ColorHex = "#007BFF" },
            new() { Name = "Business", Description = "Networking events, seminars, and entrepreneurship meetups", ColorHex = "#28A745" },
            new() { Name = "Creative Arts", Description = "Art shows, music events, and creative workshops", ColorHex = "#FFC107" },
            new() { Name = "Health & Wellness", Description = "Fitness classes, wellness seminars, and health fairs", ColorHex = "#DC3545" },
        };
        db.EventCategories.AddRange(categories);
        db.SaveChanges();

        // Venues
        var venues = new List<Venue>
        {
            new() { Name = "Innovation Hub", Address = "123 Tech Drive", City = "Austin", State = "TX", ZipCode = "78701", MaxCapacity = 50, ContactEmail = "info@innovationhub.com", ContactPhone = "(512) 555-0101", Notes = "Intimate venue with projector and whiteboard", CreatedAt = DateTime.UtcNow },
            new() { Name = "Grand Convention Center", Address = "456 Main Street", City = "Austin", State = "TX", ZipCode = "78702", MaxCapacity = 500, ContactEmail = "events@grandcc.com", ContactPhone = "(512) 555-0202", Notes = "Large venue with multiple halls and breakout rooms", CreatedAt = DateTime.UtcNow },
            new() { Name = "Riverside Pavilion", Address = "789 River Road", City = "Austin", State = "TX", ZipCode = "78703", MaxCapacity = 200, ContactEmail = "bookings@riverside.com", ContactPhone = "(512) 555-0303", Notes = "Outdoor/indoor venue with scenic river views", CreatedAt = DateTime.UtcNow },
        };
        db.Venues.AddRange(venues);
        db.SaveChanges();

        var now = DateTime.UtcNow;

        // Events
        // 1. Upcoming Published event - Tech Conference (7 days out)
        var event1 = new Event
        {
            Title = "Austin Tech Summit 2026",
            Description = "A two-day technology summit featuring keynotes from industry leaders, hands-on workshops, and networking sessions. Topics include AI, cloud computing, and DevOps.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[1].Id,
            StartDate = now.AddDays(14),
            EndDate = now.AddDays(15),
            RegistrationOpenDate = now.AddDays(-30),
            RegistrationCloseDate = now.AddDays(12),
            EarlyBirdDeadline = now.AddDays(-5),
            TotalCapacity = 200,
            CurrentRegistrations = 45,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = true,
            CreatedAt = now.AddDays(-35),
            UpdatedAt = now
        };

        // 2. Upcoming Published event - Business Networking (5 days out)
        var event2 = new Event
        {
            Title = "Startup Founders Meetup",
            Description = "Monthly meetup for startup founders and aspiring entrepreneurs. Share ideas, get feedback, and connect with potential collaborators and investors.",
            EventCategoryId = categories[1].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(5),
            EndDate = now.AddDays(5).AddHours(3),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.AddDays(4),
            TotalCapacity = 40,
            CurrentRegistrations = 18,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now
        };

        // 3. SoldOut event with waitlist
        var event3 = new Event
        {
            Title = "Creative Coding Workshop",
            Description = "An immersive workshop exploring the intersection of art and technology. Learn generative art, creative coding with p5.js, and interactive installations.",
            EventCategoryId = categories[2].Id,
            VenueId = venues[0].Id,
            StartDate = now.AddDays(10),
            EndDate = now.AddDays(10).AddHours(6),
            RegistrationOpenDate = now.AddDays(-21),
            RegistrationCloseDate = now.AddDays(9),
            EarlyBirdDeadline = now.AddDays(-7),
            TotalCapacity = 30,
            CurrentRegistrations = 30,
            WaitlistCount = 3,
            Status = EventStatus.SoldOut,
            IsFeatured = true,
            CreatedAt = now.AddDays(-25),
            UpdatedAt = now
        };

        // 4. Today/tomorrow event for check-in testing
        var event4 = new Event
        {
            Title = "Morning Yoga & Mindfulness",
            Description = "Start your day with guided yoga and mindfulness meditation. All levels welcome. Mats provided.",
            EventCategoryId = categories[3].Id,
            VenueId = venues[2].Id,
            StartDate = now.Date.AddHours(8),
            EndDate = now.Date.AddHours(17),
            RegistrationOpenDate = now.AddDays(-14),
            RegistrationCloseDate = now.Date.AddHours(-1),
            TotalCapacity = 50,
            CurrentRegistrations = 25,
            WaitlistCount = 0,
            Status = EventStatus.Published,
            IsFeatured = false,
            CreatedAt = now.AddDays(-14),
            UpdatedAt = now
        };

        // 5. Completed event (past)
        var event5 = new Event
        {
            Title = "Data Science Bootcamp",
            Description = "Intensive two-day bootcamp covering Python, pandas, machine learning basics, and data visualization. Ideal for beginners and intermediate data enthusiasts.",
            EventCategoryId = categories[0].Id,
            VenueId = venues[2].Id,
            StartDate = now.AddDays(-14),
            EndDate = now.AddDays(-13),
            RegistrationOpenDate = now.AddDays(-60),
            RegistrationCloseDate = now.AddDays(-15),
            EarlyBirdDeadline = now.AddDays(-30),
            TotalCapacity = 100,
            CurrentRegistrations = 72,
            WaitlistCount = 0,
            Status = EventStatus.Completed,
            IsFeatured = false,
            CreatedAt = now.AddDays(-65),
            UpdatedAt = now.AddDays(-13)
        };

        // 6. Draft event
        var event6 = new Event
        {
            Title = "Holiday Craft Fair",
            Description = "Annual community craft fair featuring local artisans, food vendors, and live music. A family-friendly event celebrating creativity and community.",
            EventCategoryId = categories[2].Id,
            VenueId = venues[1].Id,
            StartDate = now.AddDays(60),
            EndDate = now.AddDays(60).AddHours(8),
            RegistrationOpenDate = now.AddDays(30),
            RegistrationCloseDate = now.AddDays(58),
            TotalCapacity = 300,
            CurrentRegistrations = 0,
            WaitlistCount = 0,
            Status = EventStatus.Draft,
            IsFeatured = false,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now
        };

        var events = new List<Event> { event1, event2, event3, event4, event5, event6 };
        db.Events.AddRange(events);
        db.SaveChanges();

        // Ticket Types
        var ticketTypes = new List<TicketType>
        {
            // Event 1 - Tech Summit
            new() { EventId = event1.Id, Name = "General Admission", Price = 25.00m, EarlyBirdPrice = 15.00m, Quantity = 150, QuantitySold = 35, SortOrder = 0, IsActive = true },
            new() { EventId = event1.Id, Name = "VIP", Description = "Includes front-row seating and exclusive networking lunch", Price = 75.00m, EarlyBirdPrice = 50.00m, Quantity = 30, QuantitySold = 8, SortOrder = 1, IsActive = true },
            new() { EventId = event1.Id, Name = "Student", Description = "Valid student ID required at check-in", Price = 10.00m, EarlyBirdPrice = 5.00m, Quantity = 20, QuantitySold = 2, SortOrder = 2, IsActive = true },

            // Event 2 - Startup Meetup
            new() { EventId = event2.Id, Name = "General Admission", Price = 0.00m, Quantity = 30, QuantitySold = 12, SortOrder = 0, IsActive = true },
            new() { EventId = event2.Id, Name = "VIP", Description = "Includes dinner with featured speakers", Price = 35.00m, Quantity = 8, QuantitySold = 5, SortOrder = 1, IsActive = true },
            new() { EventId = event2.Id, Name = "Student", Price = 0.00m, Quantity = 10, QuantitySold = 1, SortOrder = 2, IsActive = true },

            // Event 3 - Creative Coding (sold out)
            new() { EventId = event3.Id, Name = "General Admission", Price = 40.00m, EarlyBirdPrice = 25.00m, Quantity = 20, QuantitySold = 20, SortOrder = 0, IsActive = true },
            new() { EventId = event3.Id, Name = "VIP", Description = "Includes workshop materials kit", Price = 80.00m, EarlyBirdPrice = 60.00m, Quantity = 8, QuantitySold = 8, SortOrder = 1, IsActive = true },
            new() { EventId = event3.Id, Name = "Student", Price = 20.00m, EarlyBirdPrice = 10.00m, Quantity = 5, QuantitySold = 2, SortOrder = 2, IsActive = true },

            // Event 4 - Yoga
            new() { EventId = event4.Id, Name = "General Admission", Price = 15.00m, Quantity = 40, QuantitySold = 20, SortOrder = 0, IsActive = true },
            new() { EventId = event4.Id, Name = "Premium", Description = "Includes premium mat and water bottle", Price = 30.00m, Quantity = 10, QuantitySold = 5, SortOrder = 1, IsActive = true },
            new() { EventId = event4.Id, Name = "Drop-In", Price = 20.00m, Quantity = 10, QuantitySold = 0, SortOrder = 2, IsActive = true },

            // Event 5 - Data Science (completed)
            new() { EventId = event5.Id, Name = "General Admission", Price = 50.00m, EarlyBirdPrice = 35.00m, Quantity = 70, QuantitySold = 52, SortOrder = 0, IsActive = true },
            new() { EventId = event5.Id, Name = "VIP", Description = "Includes 1-on-1 mentoring session", Price = 120.00m, EarlyBirdPrice = 90.00m, Quantity = 20, QuantitySold = 15, SortOrder = 1, IsActive = true },
            new() { EventId = event5.Id, Name = "Student", Price = 25.00m, EarlyBirdPrice = 15.00m, Quantity = 20, QuantitySold = 5, SortOrder = 2, IsActive = true },

            // Event 6 - Craft Fair (draft)
            new() { EventId = event6.Id, Name = "General Admission", Price = 10.00m, Quantity = 200, QuantitySold = 0, SortOrder = 0, IsActive = true },
            new() { EventId = event6.Id, Name = "VIP", Description = "Early access and goodie bag", Price = 30.00m, Quantity = 50, QuantitySold = 0, SortOrder = 1, IsActive = true },
            new() { EventId = event6.Id, Name = "Family Pass", Description = "Admits up to 4 family members", Price = 25.00m, Quantity = 50, QuantitySold = 0, SortOrder = 2, IsActive = true },
        };
        db.TicketTypes.AddRange(ticketTypes);
        db.SaveChanges();

        // Attendees
        var attendees = new List<Attendee>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "(512) 555-1001", Organization = "TechCorp", CreatedAt = now.AddDays(-40), UpdatedAt = now },
            new() { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "(512) 555-1002", Organization = "StartupXYZ", DietaryNeeds = "Vegetarian", CreatedAt = now.AddDays(-38), UpdatedAt = now },
            new() { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "(512) 555-1003", Organization = "DesignStudio", CreatedAt = now.AddDays(-36), UpdatedAt = now },
            new() { FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Organization = "University of Texas", DietaryNeeds = "Gluten-free", CreatedAt = now.AddDays(-35), UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Davis", Email = "emma.davis@example.com", Phone = "(512) 555-1005", Organization = "HealthFirst", CreatedAt = now.AddDays(-30), UpdatedAt = now },
            new() { FirstName = "Frank", LastName = "Garcia", Email = "frank.garcia@example.com", Phone = "(512) 555-1006", CreatedAt = now.AddDays(-28), UpdatedAt = now },
            new() { FirstName = "Grace", LastName = "Martinez", Email = "grace.martinez@example.com", Organization = "ArtSpace", DietaryNeeds = "Vegan", CreatedAt = now.AddDays(-25), UpdatedAt = now },
            new() { FirstName = "Henry", LastName = "Wilson", Email = "henry.wilson@example.com", Phone = "(512) 555-1008", Organization = "DataDriven Inc", CreatedAt = now.AddDays(-22), UpdatedAt = now },
            new() { FirstName = "Ivy", LastName = "Anderson", Email = "ivy.anderson@example.com", Phone = "(512) 555-1009", Organization = "YogaLife", CreatedAt = now.AddDays(-20), UpdatedAt = now },
            new() { FirstName = "Jack", LastName = "Taylor", Email = "jack.taylor@example.com", Phone = "(512) 555-1010", Organization = "FreelanceDevs", DietaryNeeds = "Nut allergy", CreatedAt = now.AddDays(-18), UpdatedAt = now },
            new() { FirstName = "Karen", LastName = "Thomas", Email = "karen.thomas@example.com", Phone = "(512) 555-1011", CreatedAt = now.AddDays(-15), UpdatedAt = now },
            new() { FirstName = "Leo", LastName = "Jackson", Email = "leo.jackson@example.com", Phone = "(512) 555-1012", Organization = "CloudNine", CreatedAt = now.AddDays(-12), UpdatedAt = now },
        };
        db.Attendees.AddRange(attendees);
        db.SaveChanges();

        // Registrations - use ticket type references
        var tt1GA = ticketTypes[0];  // Event1 GA
        var tt1VIP = ticketTypes[1]; // Event1 VIP
        var tt1Stu = ticketTypes[2]; // Event1 Student
        var tt2GA = ticketTypes[3];  // Event2 GA
        var tt2VIP = ticketTypes[4]; // Event2 VIP
        var tt3GA = ticketTypes[6];  // Event3 GA
        var tt3VIP = ticketTypes[7]; // Event3 VIP
        var tt4GA = ticketTypes[9];  // Event4 GA
        var tt4Prem = ticketTypes[10]; // Event4 Premium
        var tt5GA = ticketTypes[12]; // Event5 GA
        var tt5VIP = ticketTypes[13]; // Event5 VIP

        var registrations = new List<Registration>
        {
            // Event 1 - Tech Summit registrations
            new() { EventId = event1.Id, AttendeeId = attendees[0].Id, TicketTypeId = tt1GA.Id, ConfirmationNumber = $"SPK-{event1.StartDate:yyyyMMdd}-0001", Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-20), CreatedAt = now.AddDays(-20), UpdatedAt = now },
            new() { EventId = event1.Id, AttendeeId = attendees[1].Id, TicketTypeId = tt1VIP.Id, ConfirmationNumber = $"SPK-{event1.StartDate:yyyyMMdd}-0002", Status = RegistrationStatus.Confirmed, AmountPaid = 50.00m, RegistrationDate = now.AddDays(-18), SpecialRequests = "Need vegetarian lunch option", CreatedAt = now.AddDays(-18), UpdatedAt = now },
            new() { EventId = event1.Id, AttendeeId = attendees[3].Id, TicketTypeId = tt1Stu.Id, ConfirmationNumber = $"SPK-{event1.StartDate:yyyyMMdd}-0003", Status = RegistrationStatus.Confirmed, AmountPaid = 5.00m, RegistrationDate = now.AddDays(-15), CreatedAt = now.AddDays(-15), UpdatedAt = now },
            new() { EventId = event1.Id, AttendeeId = attendees[7].Id, TicketTypeId = tt1GA.Id, ConfirmationNumber = $"SPK-{event1.StartDate:yyyyMMdd}-0004", Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now },

            // Event 2 - Startup Meetup registrations
            new() { EventId = event2.Id, AttendeeId = attendees[1].Id, TicketTypeId = tt2GA.Id, ConfirmationNumber = $"SPK-{event2.StartDate:yyyyMMdd}-0001", Status = RegistrationStatus.Confirmed, AmountPaid = 0.00m, RegistrationDate = now.AddDays(-10), CreatedAt = now.AddDays(-10), UpdatedAt = now },
            new() { EventId = event2.Id, AttendeeId = attendees[5].Id, TicketTypeId = tt2VIP.Id, ConfirmationNumber = $"SPK-{event2.StartDate:yyyyMMdd}-0002", Status = RegistrationStatus.Confirmed, AmountPaid = 35.00m, RegistrationDate = now.AddDays(-8), CreatedAt = now.AddDays(-8), UpdatedAt = now },
            new() { EventId = event2.Id, AttendeeId = attendees[9].Id, TicketTypeId = tt2GA.Id, ConfirmationNumber = $"SPK-{event2.StartDate:yyyyMMdd}-0003", Status = RegistrationStatus.Cancelled, AmountPaid = 0.00m, RegistrationDate = now.AddDays(-7), CancellationDate = now.AddDays(-5), CancellationReason = "Schedule conflict", CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-5) },

            // Event 3 - Creative Coding (sold out) registrations + waitlist
            new() { EventId = event3.Id, AttendeeId = attendees[2].Id, TicketTypeId = tt3GA.Id, ConfirmationNumber = $"SPK-{event3.StartDate:yyyyMMdd}-0001", Status = RegistrationStatus.Confirmed, AmountPaid = 25.00m, RegistrationDate = now.AddDays(-18), CreatedAt = now.AddDays(-18), UpdatedAt = now },
            new() { EventId = event3.Id, AttendeeId = attendees[6].Id, TicketTypeId = tt3VIP.Id, ConfirmationNumber = $"SPK-{event3.StartDate:yyyyMMdd}-0002", Status = RegistrationStatus.Confirmed, AmountPaid = 60.00m, RegistrationDate = now.AddDays(-16), SpecialRequests = "Vegan snacks please", CreatedAt = now.AddDays(-16), UpdatedAt = now },
            new() { EventId = event3.Id, AttendeeId = attendees[10].Id, TicketTypeId = tt3GA.Id, ConfirmationNumber = $"SPK-{event3.StartDate:yyyyMMdd}-0003", Status = RegistrationStatus.Waitlisted, AmountPaid = 40.00m, WaitlistPosition = 1, RegistrationDate = now.AddDays(-5), CreatedAt = now.AddDays(-5), UpdatedAt = now },
            new() { EventId = event3.Id, AttendeeId = attendees[11].Id, TicketTypeId = tt3GA.Id, ConfirmationNumber = $"SPK-{event3.StartDate:yyyyMMdd}-0004", Status = RegistrationStatus.Waitlisted, AmountPaid = 40.00m, WaitlistPosition = 2, RegistrationDate = now.AddDays(-4), CreatedAt = now.AddDays(-4), UpdatedAt = now },
            new() { EventId = event3.Id, AttendeeId = attendees[4].Id, TicketTypeId = tt3GA.Id, ConfirmationNumber = $"SPK-{event3.StartDate:yyyyMMdd}-0005", Status = RegistrationStatus.Waitlisted, AmountPaid = 40.00m, WaitlistPosition = 3, RegistrationDate = now.AddDays(-3), CreatedAt = now.AddDays(-3), UpdatedAt = now },

            // Event 4 - Yoga (today) registrations with some checked in
            new() { EventId = event4.Id, AttendeeId = attendees[4].Id, TicketTypeId = tt4GA.Id, ConfirmationNumber = $"SPK-{event4.StartDate:yyyyMMdd}-0001", Status = RegistrationStatus.CheckedIn, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-10), CheckInTime = now.AddHours(-1), CreatedAt = now.AddDays(-10), UpdatedAt = now },
            new() { EventId = event4.Id, AttendeeId = attendees[8].Id, TicketTypeId = tt4Prem.Id, ConfirmationNumber = $"SPK-{event4.StartDate:yyyyMMdd}-0002", Status = RegistrationStatus.CheckedIn, AmountPaid = 30.00m, RegistrationDate = now.AddDays(-8), CheckInTime = now.AddHours(-1), CreatedAt = now.AddDays(-8), UpdatedAt = now },
            new() { EventId = event4.Id, AttendeeId = attendees[0].Id, TicketTypeId = tt4GA.Id, ConfirmationNumber = $"SPK-{event4.StartDate:yyyyMMdd}-0003", Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-6), CreatedAt = now.AddDays(-6), UpdatedAt = now },
            new() { EventId = event4.Id, AttendeeId = attendees[5].Id, TicketTypeId = tt4GA.Id, ConfirmationNumber = $"SPK-{event4.StartDate:yyyyMMdd}-0004", Status = RegistrationStatus.Confirmed, AmountPaid = 15.00m, RegistrationDate = now.AddDays(-4), CreatedAt = now.AddDays(-4), UpdatedAt = now },

            // Event 5 - Data Science (completed) registrations
            new() { EventId = event5.Id, AttendeeId = attendees[0].Id, TicketTypeId = tt5GA.Id, ConfirmationNumber = $"SPK-{event5.StartDate:yyyyMMdd}-0001", Status = RegistrationStatus.CheckedIn, AmountPaid = 35.00m, RegistrationDate = now.AddDays(-50), CheckInTime = now.AddDays(-14), CreatedAt = now.AddDays(-50), UpdatedAt = now.AddDays(-14) },
            new() { EventId = event5.Id, AttendeeId = attendees[2].Id, TicketTypeId = tt5VIP.Id, ConfirmationNumber = $"SPK-{event5.StartDate:yyyyMMdd}-0002", Status = RegistrationStatus.CheckedIn, AmountPaid = 90.00m, RegistrationDate = now.AddDays(-45), CheckInTime = now.AddDays(-14), CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-14) },
            new() { EventId = event5.Id, AttendeeId = attendees[7].Id, TicketTypeId = tt5GA.Id, ConfirmationNumber = $"SPK-{event5.StartDate:yyyyMMdd}-0003", Status = RegistrationStatus.CheckedIn, AmountPaid = 50.00m, RegistrationDate = now.AddDays(-25), CheckInTime = now.AddDays(-14), CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-14) },
            new() { EventId = event5.Id, AttendeeId = attendees[9].Id, TicketTypeId = tt5GA.Id, ConfirmationNumber = $"SPK-{event5.StartDate:yyyyMMdd}-0004", Status = RegistrationStatus.NoShow, AmountPaid = 50.00m, RegistrationDate = now.AddDays(-22), CreatedAt = now.AddDays(-22), UpdatedAt = now.AddDays(-14) },
        };
        db.Registrations.AddRange(registrations);
        db.SaveChanges();

        // Check-ins
        var checkIns = new List<CheckIn>
        {
            new() { RegistrationId = registrations[12].Id, CheckInTime = now.AddHours(-1), CheckedInBy = "Staff: Maria", Notes = "Arrived early" },
            new() { RegistrationId = registrations[13].Id, CheckInTime = now.AddHours(-1), CheckedInBy = "Staff: Maria" },
            new() { RegistrationId = registrations[16].Id, CheckInTime = now.AddDays(-14).AddHours(1), CheckedInBy = "Staff: James", Notes = "First to arrive" },
            new() { RegistrationId = registrations[17].Id, CheckInTime = now.AddDays(-14).AddHours(1), CheckedInBy = "Staff: James" },
            new() { RegistrationId = registrations[18].Id, CheckInTime = now.AddDays(-14).AddHours(2), CheckedInBy = "Staff: Sarah", Notes = "Late arrival" },
        };
        db.CheckIns.AddRange(checkIns);
        db.SaveChanges();
    }
}
