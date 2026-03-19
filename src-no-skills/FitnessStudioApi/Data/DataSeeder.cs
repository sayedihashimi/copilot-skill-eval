using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext context)
    {
        if (await context.MembershipPlans.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basicPlan = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, IsActive = true, CreatedAt = now, UpdatedAt = now
        };
        var premiumPlan = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now
        };
        var elitePlan = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes and premium amenities",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now
        };
        context.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        await context.SaveChangesAsync();

        // Members
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102",
                JoinDate = today.AddMonths(-6), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201",
                DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lin Chen", EmergencyContactPhone = "555-0202",
                JoinDate = today.AddMonths(-4), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sarah", LastName = "Williams", Email = "sarah.williams@email.com", Phone = "555-0301",
                DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Tom Williams", EmergencyContactPhone = "555-0302",
                JoinDate = today.AddMonths(-3), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Rodriguez", Email = "james.rodriguez@email.com", Phone = "555-0401",
                DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Maria Rodriguez", EmergencyContactPhone = "555-0402",
                JoinDate = today.AddMonths(-5), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0501",
                DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "John Davis", EmergencyContactPhone = "555-0502",
                JoinDate = today.AddMonths(-2), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0601",
                DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Soo Kim", EmergencyContactPhone = "555-0602",
                JoinDate = today.AddMonths(-1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0701",
                DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "Mike Brown", EmergencyContactPhone = "555-0702",
                JoinDate = today.AddMonths(-7), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ryan", LastName = "Taylor", Email = "ryan.taylor@email.com", Phone = "555-0801",
                DateOfBirth = new DateOnly(1987, 4, 18), EmergencyContactName = "Lisa Taylor", EmergencyContactPhone = "555-0802",
                JoinDate = today.AddMonths(-8), IsActive = false, CreatedAt = now, UpdatedAt = now }
        };
        context.Members.AddRange(members);
        await context.SaveChangesAsync();

        // Memberships
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-15), EndDate = today.AddDays(15),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-10), EndDate = today.AddDays(20),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-5), EndDate = today.AddDays(25),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-20), EndDate = today.AddDays(10),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-8), EndDate = today.AddDays(22),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-3), EndDate = today.AddDays(27),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Frozen membership
            new() { MemberId = members[6].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-25), EndDate = today.AddDays(5),
                Status = MembershipStatus.Frozen, PaymentStatus = PaymentStatus.Paid,
                FreezeStartDate = today.AddDays(-2), FreezeEndDate = today.AddDays(12),
                CreatedAt = now, UpdatedAt = now },
            // Expired membership for inactive member
            new() { MemberId = members[7].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now }
        };
        context.Memberships.AddRange(memberships);
        await context.SaveChangesAsync();

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Maya", LastName = "Patel", Email = "maya.patel@zenith.com", Phone = "555-1001",
                Bio = "Certified yoga instructor with 10 years of experience in Vinyasa and Hatha yoga.",
                Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15),
                IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jake", LastName = "Thompson", Email = "jake.thompson@zenith.com", Phone = "555-1002",
                Bio = "Former professional athlete specializing in high-intensity training and strength conditioning.",
                Specializations = "HIIT, Boxing, Strength Training", HireDate = new DateOnly(2021, 3, 1),
                IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sophia", LastName = "Nguyen", Email = "sophia.nguyen@zenith.com", Phone = "555-1003",
                Bio = "Passionate spin and cardio instructor who brings energy and motivation to every class.",
                Specializations = "Spin, HIIT, Cardio", HireDate = new DateOnly(2019, 6, 20),
                IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Lucas", LastName = "Martinez", Email = "lucas.martinez@zenith.com", Phone = "555-1004",
                Bio = "Mindfulness and movement specialist with expertise in Pilates and meditation techniques.",
                Specializations = "Pilates, Meditation, Yoga", HireDate = new DateOnly(2022, 2, 10),
                IsActive = true, CreatedAt = now, UpdatedAt = now }
        };
        context.Instructors.AddRange(instructors);
        await context.SaveChangesAsync();

        // Class Types
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Traditional yoga focusing on flexibility, balance, and mindfulness",
                DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250,
                DifficultyLevel = DifficultyLevel.AllLevels, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn",
                DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500,
                DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class with energizing music and varied intensity",
                DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 450,
                DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core-strengthening exercises for improved posture and flexibility",
                DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300,
                DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing class with personal technique coaching",
                DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600,
                DifficultyLevel = DifficultyLevel.Advanced, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and breathwork session",
                DefaultDurationMinutes = 30, DefaultCapacity = 20, IsPremium = true, CaloriesPerSession = 50,
                DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };
        context.ClassTypes.AddRange(classTypes);
        await context.SaveChangesAsync();

        // Class Schedules - spread over the next 7 days
        var baseDate = now.Date;
        var schedules = new List<ClassSchedule>
        {
            // Today
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = baseDate.AddHours(8), EndTime = baseDate.AddHours(9),
                Capacity = 20, CurrentEnrollment = 5, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Today - full class with waitlist
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = baseDate.AddHours(10), EndTime = baseDate.AddHours(10).AddMinutes(45),
                Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Main Floor",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Tomorrow
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(7).AddMinutes(45),
                Capacity = 12, CurrentEnrollment = 8, WaitlistCount = 0, Room = "Studio B",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Tomorrow
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(10),
                Capacity = 15, CurrentEnrollment = 10, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 2 - premium boxing
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = baseDate.AddDays(2).AddHours(11), EndTime = baseDate.AddDays(2).AddHours(12),
                Capacity = 10, CurrentEnrollment = 6, WaitlistCount = 0, Room = "Main Floor",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 2 - premium meditation
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(17).AddMinutes(30),
                Capacity = 20, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 3
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[3].Id,
                StartTime = baseDate.AddDays(3).AddHours(8), EndTime = baseDate.AddDays(3).AddHours(9),
                Capacity = 20, CurrentEnrollment = 12, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 3
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = baseDate.AddDays(3).AddHours(18), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(45),
                Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Main Floor",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 4
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[2].Id,
                StartTime = baseDate.AddDays(4).AddHours(6), EndTime = baseDate.AddDays(4).AddHours(6).AddMinutes(45),
                Capacity = 12, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio B",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 5
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[3].Id,
                StartTime = baseDate.AddDays(5).AddHours(10), EndTime = baseDate.AddDays(5).AddHours(11),
                Capacity = 15, CurrentEnrollment = 4, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Day 6
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = baseDate.AddDays(6).AddHours(9), EndTime = baseDate.AddDays(6).AddHours(10),
                Capacity = 10, CurrentEnrollment = 7, WaitlistCount = 0, Room = "Main Floor",
                Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = baseDate.AddDays(4).AddHours(16), EndTime = baseDate.AddDays(4).AddHours(17),
                Capacity = 20, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable",
                CreatedAt = now, UpdatedAt = now }
        };
        context.ClassSchedules.AddRange(schedules);
        await context.SaveChangesAsync();

        // Bookings
        var bookings = new List<Booking>
        {
            // Confirmed bookings for first class (schedule 0 - Yoga today)
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-2),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, BookingDate = now.AddHours(-3),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, BookingDate = now.AddHours(-1),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Full HIIT class with waitlist (schedule 1)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-5),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, BookingDate = now.AddHours(-4),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-3),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-2),
                Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-1),
                Status = BookingStatus.Waitlisted, WaitlistPosition = 2, CreatedAt = now, UpdatedAt = now },

            // Spin class bookings (schedule 2)
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-6),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-5),
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Cancelled booking
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, BookingDate = now.AddDays(-1),
                Status = BookingStatus.Cancelled, CancellationDate = now.AddHours(-12),
                CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // Attended booking (past-ish)
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, BookingDate = now.AddDays(-1),
                Status = BookingStatus.Attended, CheckInTime = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },

            // No-show booking
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[3].Id, BookingDate = now.AddDays(-1),
                Status = BookingStatus.NoShow, CreatedAt = now, UpdatedAt = now },

            // Future bookings
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, BookingDate = now,
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, BookingDate = now,
                Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
        };
        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }
}
