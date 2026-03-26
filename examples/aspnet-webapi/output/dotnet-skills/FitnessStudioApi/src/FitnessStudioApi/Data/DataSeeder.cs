using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static void Seed(FitnessDbContext context)
    {
        if (context.MembershipPlans.Any())
            return; // Database already seeded

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // === Membership Plans ===
        var basicPlan = new MembershipPlan
        {
            Name = "Basic",
            Description = "Access to standard classes with limited weekly bookings. Perfect for beginners.",
            DurationMonths = 1,
            Price = 29.99m,
            MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var premiumPlan = new MembershipPlan
        {
            Name = "Premium",
            Description = "Full access to all classes including premium. Great for regulars.",
            DurationMonths = 1,
            Price = 49.99m,
            MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var elitePlan = new MembershipPlan
        {
            Name = "Elite",
            Description = "Unlimited access to all classes. The ultimate fitness experience.",
            DurationMonths = 1,
            Price = 79.99m,
            MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        context.SaveChanges();

        // === Members ===
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0103", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0104", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com", Phone = "555-0105", DateOfBirth = new DateOnly(1995, 11, 8), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0106", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0107", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0108", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Priya", LastName = "Patel", Email = "priya.patel@email.com", Phone = "555-0109", DateOfBirth = new DateOnly(1992, 5, 14), EmergencyContactName = "Raj Patel", EmergencyContactPhone = "555-0110", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Daniel", LastName = "Kim", Email = "daniel.kim@email.com", Phone = "555-0111", DateOfBirth = new DateOnly(1998, 9, 3), EmergencyContactName = "Susan Kim", EmergencyContactPhone = "555-0112", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Thompson", Email = "emma.thompson@email.com", Phone = "555-0113", DateOfBirth = new DateOnly(1993, 12, 20), EmergencyContactName = "Mark Thompson", EmergencyContactPhone = "555-0114", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ryan", LastName = "O'Brien", Email = "ryan.obrien@email.com", Phone = "555-0115", DateOfBirth = new DateOnly(1987, 4, 10), EmergencyContactName = "Katie O'Brien", EmergencyContactPhone = "555-0116", JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now }
        };

        context.Members.AddRange(members);
        context.SaveChanges();

        // === Memberships ===
        var memberships = new List<Membership>
        {
            // Alice - Basic, Active
            new() { MemberId = members[0].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0).AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Marcus - Premium, Active
            new() { MemberId = members[1].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Sofia - Elite, Active
            new() { MemberId = members[2].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // James - Premium, Active
            new() { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Priya - Elite, Active
            new() { MemberId = members[4].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddDays(-3), EndDate = today.AddDays(27), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Daniel - Basic, Active
            new() { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddDays(-7), EndDate = today.AddDays(23), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Emma - Premium, Expired (old membership)
            new() { MemberId = members[6].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now.AddMonths(-3), UpdatedAt = now.AddMonths(-2) },
            // Emma - Elite, Active (current)
            new() { MemberId = members[6].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddDays(-2), EndDate = today.AddDays(28), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Ryan - Basic, Expired
            new() { MemberId = members[7].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now.AddMonths(-2), UpdatedAt = now.AddMonths(-1) }
        };

        context.Memberships.AddRange(memberships);
        context.SaveChanges();

        // === Instructors ===
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Maya", LastName = "Singh", Email = "maya.singh@zenithfitness.com", Phone = "555-0201", Bio = "Certified yoga and meditation instructor with 10+ years of experience. Specializes in Vinyasa and Restorative yoga.", Specializations = "Yoga, Meditation, Pilates", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Tyler", LastName = "Brooks", Email = "tyler.brooks@zenithfitness.com", Phone = "555-0202", Bio = "Former competitive cyclist and HIIT specialist. Known for high-energy, results-driven classes.", Specializations = "HIIT, Spin, Boxing", HireDate = new DateOnly(2021, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Leah", LastName = "Foster", Email = "leah.foster@zenithfitness.com", Phone = "555-0203", Bio = "Pilates and barre certified instructor focusing on core strength and flexibility.", Specializations = "Pilates, Yoga", HireDate = new DateOnly(2022, 3, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jordan", LastName = "Blake", Email = "jordan.blake@zenithfitness.com", Phone = "555-0204", Bio = "Professional boxing coach and strength training expert. Brings intensity and motivation to every session.", Specializations = "Boxing, HIIT", HireDate = new DateOnly(2021, 9, 20), CreatedAt = now, UpdatedAt = now }
        };

        context.Instructors.AddRange(instructors);
        context.SaveChanges();

        // === Class Types ===
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "A mindful practice combining breath work, flexibility, and strength through flowing poses.", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-Intensity Interval Training that alternates between intense bursts of activity and recovery.", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class with varying intensity levels set to energizing music.", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core-focused exercise method improving flexibility, strength, and body awareness.", DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing fitness class combining technique drills, bag work, and conditioning.", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and mindfulness session for deep relaxation and mental clarity.", DefaultDurationMinutes = 30, DefaultCapacity = 20, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now }
        };

        context.ClassTypes.AddRange(classTypes);
        context.SaveChanges();

        // === Class Schedules (next 7 days) ===
        var tomorrow = now.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Day 1 - Tomorrow
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddHours(11), EndTime = tomorrow.AddHours(12), Capacity = 10, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },

            // Day 2
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(6), EndTime = tomorrow.AddDays(1).AddHours(6).AddMinutes(45), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(10).AddMinutes(50), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(1).AddHours(18), EndTime = tomorrow.AddDays(1).AddHours(18).AddMinutes(30), Capacity = 20, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },

            // Day 3
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8), Capacity = 20, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(12).AddMinutes(45), Capacity = 15, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },

            // Day 4 - Full class with waitlist
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(3).AddHours(8), EndTime = tomorrow.AddDays(3).AddHours(8).AddMinutes(45), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },

            // Day 5
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(4).AddHours(17), EndTime = tomorrow.AddDays(4).AddHours(18), Capacity = 10, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(4).AddHours(10), EndTime = tomorrow.AddDays(4).AddHours(10).AddMinutes(50), Capacity = 15, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },

            // Cancelled class
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(5).AddHours(7), EndTime = tomorrow.AddDays(5).AddHours(8), Capacity = 20, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now }
        };

        context.ClassSchedules.AddRange(schedules);
        context.SaveChanges();

        // === Bookings ===
        var bookings = new List<Booking>
        {
            // Confirmed bookings for tomorrow's Yoga (schedule 0)
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-5), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Confirmed bookings for tomorrow's HIIT (schedule 1)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Confirmed bookings for Boxing premium class (schedule 2)
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[1].Id, BookingDate = now.AddHours(-12), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-6), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Spin class bookings (schedule 3)
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Pilates booking (schedule 4)
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[6].Id, BookingDate = now.AddHours(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Meditation premium class bookings (schedule 5)
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-4), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[6].Id, BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Day 3 Yoga booking (schedule 6)
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Full Spin class (schedule 8) - 3 confirmed + 2 waitlisted
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[2].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[3].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[4].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-12), Status = BookingStatus.Waitlisted, WaitlistPosition = 2, CreatedAt = now, UpdatedAt = now },

            // Boxing class booking (schedule 9)
            new() { ClassScheduleId = schedules[9].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-8), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Cancelled booking
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[5].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Cancelled, CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now }
        };

        context.Bookings.AddRange(bookings);
        context.SaveChanges();
    }
}
