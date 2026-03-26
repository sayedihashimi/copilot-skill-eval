using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return; // Already seeded

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // --- Membership Plans ---
        var basicPlan = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now
        };
        var premiumPlan = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        var elitePlan = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes, priority booking",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        db.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        await db.SaveChangesAsync();

        // --- Members ---
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102",
                JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0103",
                DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lin Chen", EmergencyContactPhone = "555-0104",
                JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com", Phone = "555-0105",
                DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0106",
                JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0107",
                DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0108",
                JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Emma", LastName = "Thompson", Email = "emma.thompson@email.com", Phone = "555-0109",
                DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "David Thompson", EmergencyContactPhone = "555-0110",
                JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Daniel", LastName = "Kim", Email = "daniel.kim@email.com", Phone = "555-0111",
                DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Grace Kim", EmergencyContactPhone = "555-0112",
                JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0113",
                DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "Michael Brown", EmergencyContactPhone = "555-0114",
                JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Ryan", LastName = "Patel", Email = "ryan.patel@email.com", Phone = "555-0115",
                DateOfBirth = new DateOnly(1987, 6, 18), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0116",
                JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now }
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // --- Memberships ---
        // Active memberships for 6 members
        var memberships = new[]
        {
            new Membership { MemberId = members[0].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(0),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[1].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-15), EndDate = today.AddDays(15),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-10), EndDate = today.AddDays(20),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-5), EndDate = today.AddDays(25),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[4].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-20), EndDate = today.AddDays(10),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[5].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-12), EndDate = today.AddDays(18),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[6].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-8), EndDate = today.AddDays(22),
                Status = MembershipStatus.Frozen, PaymentStatus = PaymentStatus.Paid,
                FreezeStartDate = today.AddDays(-3), FreezeEndDate = today.AddDays(11),
                CreatedAt = now, UpdatedAt = now },
            // Ryan has an expired membership (no active)
            new Membership { MemberId = members[7].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid,
                CreatedAt = now, UpdatedAt = now }
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // --- Instructors ---
        var instructors = new[]
        {
            new Instructor { FirstName = "Maya", LastName = "Singh", Email = "maya.singh@zenith.com", Phone = "555-0201",
                Bio = "Certified yoga instructor with 10 years of experience in Hatha and Vinyasa flow.",
                Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15),
                CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Jake", LastName = "Morrison", Email = "jake.morrison@zenith.com", Phone = "555-0202",
                Bio = "Former collegiate athlete specializing in high-intensity training and boxing.",
                Specializations = "HIIT, Boxing, Strength Training", HireDate = new DateOnly(2021, 6, 1),
                CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Luna", LastName = "Park", Email = "luna.park@zenith.com", Phone = "555-0203",
                Bio = "Spin and cycling expert with competitive cycling background.",
                Specializations = "Spin, HIIT, Cardio", HireDate = new DateOnly(2022, 3, 10),
                CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Alex", LastName = "Rivera", Email = "alex.rivera@zenith.com", Phone = "555-0204",
                Bio = "Pilates and wellness coach focusing on mind-body connection.",
                Specializations = "Pilates, Yoga, Meditation", HireDate = new DateOnly(2019, 9, 20),
                CreatedAt = now, UpdatedAt = now }
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // --- Class Types ---
        var classTypes = new[]
        {
            new ClassType { Name = "Yoga", Description = "Traditional Hatha and Vinyasa yoga classes for flexibility and mindfulness",
                DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250,
                DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "HIIT", Description = "High-Intensity Interval Training for maximum calorie burn",
                DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500,
                DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Spin", Description = "Indoor cycling classes with motivating music and varied intensity",
                DefaultDurationMinutes = 45, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 450,
                DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Pilates", Description = "Core-strengthening exercises focusing on posture and flexibility",
                DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300,
                DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Boxing", Description = "Premium boxing classes with personal attention and advanced techniques",
                DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600,
                DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Meditation", Description = "Premium guided meditation and breathing sessions for deep relaxation",
                DefaultDurationMinutes = 30, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 50,
                DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now }
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // --- Class Schedules (next 7 days) ---
        var tomorrow = now.Date.AddDays(1);
        var schedules = new[]
        {
            // Tomorrow
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8),
                Capacity = 20, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45),
                Capacity = 15, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddHours(10), EndTime = tomorrow.AddHours(10).AddMinutes(45),
                Capacity = 25, Room = "Spin Room", CreatedAt = now, UpdatedAt = now },
            // Day after tomorrow
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(8).AddMinutes(50),
                Capacity = 15, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(1).AddHours(11), EndTime = tomorrow.AddDays(1).AddHours(12),
                Capacity = 10, Room = "Boxing Ring", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(1).AddHours(17), EndTime = tomorrow.AddDays(1).AddHours(17).AddMinutes(30),
                Capacity = 12, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            // 3 days out
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8),
                Capacity = 20, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(12).AddMinutes(45),
                Capacity = 15, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // 4 days out — full class with waitlist
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(3).AddHours(9), EndTime = tomorrow.AddDays(3).AddHours(9).AddMinutes(45),
                Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Spin Room",
                CreatedAt = now, UpdatedAt = now },
            // 5 days out
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(4).AddHours(8), EndTime = tomorrow.AddDays(4).AddHours(8).AddMinutes(50),
                Capacity = 15, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(4).AddHours(14), EndTime = tomorrow.AddDays(4).AddHours(15),
                Capacity = 10, Room = "Boxing Ring", CreatedAt = now, UpdatedAt = now },
            // 6 days out — cancelled class
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(5).AddHours(7), EndTime = tomorrow.AddDays(5).AddHours(8),
                Capacity = 20, Room = "Studio A", Status = ClassStatus.Cancelled,
                CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now }
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // --- Bookings ---
        var bookings = new List<Booking>
        {
            // Tomorrow's Yoga class — confirmed bookings
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id,
                BookingDate = now.AddHours(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id,
                BookingDate = now.AddHours(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[3].Id,
                BookingDate = now.AddMinutes(-45), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Tomorrow's HIIT — confirmed + cancelled
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id,
                BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id,
                BookingDate = now.AddHours(-2), Status = BookingStatus.Cancelled,
                CancellationDate = now.AddHours(-1), CancellationReason = "Schedule conflict",
                CreatedAt = now, UpdatedAt = now },

            // Spin class tomorrow — confirmed
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[2].Id,
                BookingDate = now.AddHours(-4), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id,
                BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Day after tomorrow's Pilates
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id,
                BookingDate = now.AddHours(-5), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Boxing (premium) — confirmed
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id,
                BookingDate = now.AddHours(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Full Spin class (capacity 3) — 3 confirmed + 2 waitlisted
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[0].Id,
                BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[1].Id,
                BookingDate = now.AddDays(-1).AddHours(1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[3].Id,
                BookingDate = now.AddDays(-1).AddHours(2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[4].Id,
                BookingDate = now.AddDays(-1).AddHours(3), Status = BookingStatus.Waitlisted, WaitlistPosition = 1,
                CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[8].Id, MemberId = members[5].Id,
                BookingDate = now.AddDays(-1).AddHours(4), Status = BookingStatus.Waitlisted, WaitlistPosition = 2,
                CreatedAt = now, UpdatedAt = now },

            // No-show booking for a past-like schedule
            new() { ClassScheduleId = schedules[9].Id, MemberId = members[3].Id,
                BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now }
        };

        db.Bookings.AddRange(bookings);

        // Update enrollment counts for classes with bookings
        schedules[0].CurrentEnrollment = 3;
        schedules[1].CurrentEnrollment = 1;
        schedules[2].CurrentEnrollment = 2;
        schedules[3].CurrentEnrollment = 1;
        schedules[4].CurrentEnrollment = 1;
        schedules[9].CurrentEnrollment = 1;

        await db.SaveChangesAsync();
    }
}
