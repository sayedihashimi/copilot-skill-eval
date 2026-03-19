using FitnessStudioApi.Models;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static void Seed(FitnessDbContext db)
    {
        if (db.MembershipPlans.Any())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // --- Membership Plans ---
        var basic = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now
        };
        var premium = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        var elite = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes, priority booking",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        db.MembershipPlans.AddRange(basic, premium, elite);
        db.SaveChanges();

        // --- Members ---
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102",
                JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0103",
                DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0104",
                JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com", Phone = "555-0105",
                DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0106",
                JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0107",
                DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Emma Williams", EmergencyContactPhone = "555-0108",
                JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Priya", LastName = "Patel", Email = "priya.patel@email.com", Phone = "555-0109",
                DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Raj Patel", EmergencyContactPhone = "555-0110",
                JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0111",
                DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Sarah Kim", EmergencyContactPhone = "555-0112",
                JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Elena", LastName = "Volkov", Email = "elena.volkov@email.com", Phone = "555-0113",
                DateOfBirth = new DateOnly(1987, 12, 3), EmergencyContactName = "Ivan Volkov", EmergencyContactPhone = "555-0114",
                JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Tyler", LastName = "Brooks", Email = "tyler.brooks@email.com", Phone = "555-0115",
                DateOfBirth = new DateOnly(2000, 6, 18), EmergencyContactName = "Jennifer Brooks", EmergencyContactPhone = "555-0116",
                JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now },
        };
        db.Members.AddRange(members);
        db.SaveChanges();

        // --- Memberships ---
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-15),
                EndDate = today.AddDays(-15).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-10),
                EndDate = today.AddDays(-10).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-20),
                EndDate = today.AddDays(-20).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-5),
                EndDate = today.AddDays(-5).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-12),
                EndDate = today.AddDays(-12).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-25),
                EndDate = today.AddDays(-25).AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            // Expired membership for Elena
            new() { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-2),
                EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now.AddMonths(-2), UpdatedAt = now },
            // Expired membership for Tyler
            new() { MemberId = members[7].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-3),
                EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now.AddMonths(-3), UpdatedAt = now },
        };
        db.Memberships.AddRange(memberships);
        db.SaveChanges();

        // --- Instructors ---
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Maya", LastName = "Thompson", Email = "maya.thompson@zenithfitness.com", Phone = "555-0201",
                Bio = "Certified yoga and meditation instructor with 10 years of experience", Specializations = "Yoga, Meditation, Pilates",
                HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Carlos", LastName = "Rivera", Email = "carlos.rivera@zenithfitness.com", Phone = "555-0202",
                Bio = "Former competitive boxer turned fitness instructor", Specializations = "Boxing, HIIT, Strength Training",
                HireDate = new DateOnly(2021, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sarah", LastName = "O'Brien", Email = "sarah.obrien@zenithfitness.com", Phone = "555-0203",
                Bio = "Spin and HIIT specialist, marathon runner", Specializations = "Spin, HIIT, Cardio",
                HireDate = new DateOnly(2019, 9, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jin", LastName = "Tanaka", Email = "jin.tanaka@zenithfitness.com", Phone = "555-0204",
                Bio = "Pilates and flexibility expert, certified physical therapist", Specializations = "Pilates, Yoga, Flexibility",
                HireDate = new DateOnly(2022, 3, 20), CreatedAt = now, UpdatedAt = now },
        };
        db.Instructors.AddRange(instructors);
        db.SaveChanges();

        // --- Class Types ---
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Vinyasa flow yoga for all levels", DefaultDurationMinutes = 60, DefaultCapacity = 20,
                IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-intensity interval training", DefaultDurationMinutes = 45, DefaultCapacity = 15,
                IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class", DefaultDurationMinutes = 45, DefaultCapacity = 25,
                IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core strengthening and flexibility", DefaultDurationMinutes = 60, DefaultCapacity = 15,
                IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing fitness class with personal attention", DefaultDurationMinutes = 60, DefaultCapacity = 10,
                IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and mindfulness", DefaultDurationMinutes = 30, DefaultCapacity = 12,
                IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
        };
        db.ClassTypes.AddRange(classTypes);
        db.SaveChanges();

        // --- Class Schedules ---
        // Create classes over the next 7 days
        var tomorrow = now.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Tomorrow
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8),
                Capacity = 20, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45),
                Capacity = 15, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddHours(10), EndTime = tomorrow.AddHours(10).AddMinutes(45),
                Capacity = 25, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day after tomorrow
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(9),
                Capacity = 15, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(1).AddHours(11), EndTime = tomorrow.AddDays(1).AddHours(12),
                Capacity = 10, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(1).AddHours(17), EndTime = tomorrow.AddDays(1).AddHours(17).AddMinutes(30),
                Capacity = 12, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // 3 days out
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8),
                Capacity = 20, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(12).AddMinutes(45),
                Capacity = 15, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // 4 days out — a FULL class (capacity 3 for demo)
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(3).AddHours(9), EndTime = tomorrow.AddDays(3).AddHours(9).AddMinutes(45),
                Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 1, Room = "Studio B",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // 5 days out
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(4).AddHours(8), EndTime = tomorrow.AddDays(4).AddHours(9),
                Capacity = 15, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(4).AddHours(16), EndTime = tomorrow.AddDays(4).AddHours(17),
                Capacity = 10, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // 6 days out — Cancelled class
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(5).AddHours(17), EndTime = tomorrow.AddDays(5).AddHours(17).AddMinutes(30),
                Capacity = 12, Room = "Studio A", Status = ClassScheduleStatus.Cancelled,
                CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
        };
        db.ClassSchedules.AddRange(schedules);
        db.SaveChanges();

        // --- Bookings ---
        // The full class (schedule index 8) needs matching bookings
        var fullClass = schedules[8];
        var bookings = new List<Booking>
        {
            // Confirmed bookings for the full class
            new() { ClassScheduleId = fullClass.Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed,
                BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = fullClass.Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed,
                BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = fullClass.Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed,
                BookingDate = now.AddHours(-12), CreatedAt = now, UpdatedAt = now },
            // Waitlisted booking for the full class
            new() { ClassScheduleId = fullClass.Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted,
                WaitlistPosition = 1, BookingDate = now.AddHours(-6), CreatedAt = now, UpdatedAt = now },

            // Bookings for tomorrow's Yoga class
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Bookings for tomorrow's HIIT class
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // A cancelled booking
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id, Status = BookingStatus.Cancelled,
                BookingDate = now.AddDays(-2), CancellationDate = now.AddDays(-1),
                CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // Bookings for Boxing class (premium) — day after tomorrow
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed,
                BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Past bookings (attended / no-show) — use a past class we'll create
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed,
                BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed,
                BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },

            // Booking for cancelled class (auto-cancelled)
            new() { ClassScheduleId = schedules[11].Id, MemberId = members[2].Id, Status = BookingStatus.Cancelled,
                BookingDate = now.AddDays(-3), CancellationDate = now.AddDays(-2),
                CancellationReason = "Class cancelled by studio", CreatedAt = now, UpdatedAt = now },
        };

        // Update enrollment counts for classes with bookings
        schedules[0].CurrentEnrollment = 3;
        schedules[1].CurrentEnrollment = 2;
        schedules[3].CurrentEnrollment = 2;
        schedules[4].CurrentEnrollment = 2;

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }
}
