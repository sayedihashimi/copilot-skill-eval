using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class DataSeeder(FitnessDbContext db, ILogger<DataSeeder> logger)
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<DataSeeder> _logger = logger;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.MembershipPlans.AnyAsync(ct))
        {
            _logger.LogInformation("Database already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding database...");

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basic = new MembershipPlan
        {
            Name = "Basic",
            Description = "Access to standard classes with limited weekly bookings",
            DurationMonths = 1,
            Price = 29.99m,
            MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var premium = new MembershipPlan
        {
            Name = "Premium",
            Description = "Access to all classes including premium with generous booking limits",
            DurationMonths = 1,
            Price = 49.99m,
            MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var elite = new MembershipPlan
        {
            Name = "Elite",
            Description = "Unlimited access to all classes and premium amenities",
            DurationMonths = 1,
            Price = 79.99m,
            MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.MembershipPlans.AddRange(basic, premium, elite);
        await _db.SaveChangesAsync(ct);

        // Members
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Sarah", LastName = "Williams", Email = "sarah.williams@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Tom Williams", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "James", LastName = "Patel", Email = "james.patel@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Nina Patel", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Emma", LastName = "Rodriguez", Email = "emma.rodriguez@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1991, 9, 3), EmergencyContactName = "Jenny Kim", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1993, 12, 19), EmergencyContactName = "Mike Brown", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Ryan", LastName = "Taylor", Email = "ryan.taylor@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 4, 25), EmergencyContactName = "Karen Taylor", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-7), IsActive = false, CreatedAt = now, UpdatedAt = now }
        };

        _db.Members.AddRange(members);
        await _db.SaveChangesAsync(ct);

        // Memberships
        var memberships = new[]
        {
            // Active memberships
            new Membership { MemberId = members[0].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[1].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[4].Id, MembershipPlanId = basic.Id, StartDate = today.AddDays(-25), EndDate = today.AddDays(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[5].Id, MembershipPlanId = elite.Id, StartDate = today.AddDays(-8), EndDate = today.AddDays(22), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Frozen membership
            new Membership { MemberId = members[6].Id, MembershipPlanId = premium.Id, StartDate = today.AddDays(-12), EndDate = today.AddDays(18), Status = MembershipStatus.Frozen, PaymentStatus = PaymentStatus.Paid, FreezeStartDate = today.AddDays(-2), FreezeEndDate = today.AddDays(12), CreatedAt = now, UpdatedAt = now },
            // Expired memberships
            new Membership { MemberId = members[7].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[0].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-8), EndDate = today.AddMonths(-7), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now }
        };

        _db.Memberships.AddRange(memberships);
        await _db.SaveChangesAsync(ct);

        // Instructors
        var instructors = new[]
        {
            new Instructor { FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience in Hatha and Vinyasa yoga.", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Jake", LastName = "Thompson", Email = "jake.thompson@zenithfitness.com", Phone = "555-1002", Bio = "Former professional athlete specializing in high-intensity training and boxing.", Specializations = "HIIT,Boxing,Spin", HireDate = new DateOnly(2021, 6, 1), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Priya", LastName = "Sharma", Email = "priya.sharma@zenithfitness.com", Phone = "555-1003", Bio = "Pilates and meditation expert with a focus on mind-body connection.", Specializations = "Pilates,Yoga,Meditation", HireDate = new DateOnly(2019, 9, 10), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Alex", LastName = "Rivera", Email = "alex.rivera@zenithfitness.com", Phone = "555-1004", Bio = "Spin and HIIT specialist dedicated to pushing limits and building endurance.", Specializations = "Spin,HIIT,Boxing", HireDate = new DateOnly(2022, 3, 20), CreatedAt = now, UpdatedAt = now }
        };

        _db.Instructors.AddRange(instructors);
        await _db.SaveChangesAsync(ct);

        // Class Types
        var yoga = new ClassType { Name = "Yoga", Description = "Traditional Hatha yoga for flexibility and relaxation", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 200, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now };
        var hiit = new ClassType { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var spin = new ClassType { Name = "Spin", Description = "Indoor cycling workout to build cardio endurance", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 400, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now };
        var pilates = new ClassType { Name = "Pilates", Description = "Core-focused workout for strength and posture", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now };
        var boxing = new ClassType { Name = "Boxing", Description = "Boxing fitness combining technique and conditioning", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var meditation = new ClassType { Name = "Meditation", Description = "Guided meditation for stress relief and mental clarity", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now };

        _db.ClassTypes.AddRange(yoga, hiit, spin, pilates, boxing, meditation);
        await _db.SaveChangesAsync(ct);

        // Class Schedules - spread over next 7 days
        var tomorrow = now.Date.AddDays(1);
        var schedules = new[]
        {
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 14, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = spin.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddHours(11), EndTime = tomorrow.AddHours(11).AddMinutes(45), Capacity = 20, CurrentEnrollment = 10, Room = "Spin Room", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = pilates.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddHours(14), EndTime = tomorrow.AddHours(15), Capacity = 15, CurrentEnrollment = 8, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 2, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = meditation.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(1).AddHours(18), EndTime = tomorrow.AddDays(1).AddHours(18).AddMinutes(30), Capacity = 25, CurrentEnrollment = 5, Room = "Zen Room", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8), Capacity = 20, CurrentEnrollment = 15, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = hiit.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(12).AddMinutes(45), Capacity = 15, CurrentEnrollment = 6, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = spin.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(3).AddHours(8), EndTime = tomorrow.AddDays(3).AddHours(8).AddMinutes(45), Capacity = 20, CurrentEnrollment = 18, Room = "Spin Room", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = pilates.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(3).AddHours(16), EndTime = tomorrow.AddDays(3).AddHours(17), Capacity = 15, CurrentEnrollment = 4, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(4).AddHours(11), EndTime = tomorrow.AddDays(4).AddHours(12), Capacity = 12, CurrentEnrollment = 7, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new ClassSchedule { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(5).AddHours(7), EndTime = tomorrow.AddDays(5).AddHours(8), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
            // More classes
            new ClassSchedule { ClassTypeId = meditation.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(5).AddHours(19), EndTime = tomorrow.AddDays(5).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 12, Room = "Zen Room", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(6).AddHours(9), EndTime = tomorrow.AddDays(6).AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 3, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
        };

        _db.ClassSchedules.AddRange(schedules);
        await _db.SaveChangesAsync(ct);

        // Bookings
        var bookings = new List<Booking>
        {
            // Confirmed bookings for tomorrow's yoga class
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },

            // HIIT class (nearly full) bookings
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-5), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },

            // Boxing class (full) with waitlist
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-11), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-10), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-9), CreatedAt = now, UpdatedAt = now },
            // Waitlisted for boxing
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[2].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },

            // Spin class bookings
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-7), CreatedAt = now, UpdatedAt = now },

            // Cancelled booking
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, Status = BookingStatus.Cancelled, CancellationDate = now.AddHours(-1), CancellationReason = "Schedule conflict", BookingDate = now.AddHours(-10), CreatedAt = now, UpdatedAt = now },

            // Meditation class
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },

            // Future bookings for later in the week
            new() { ClassScheduleId = schedules[7].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[9].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
        };

        _db.Bookings.AddRange(bookings);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Database seeded successfully with {Plans} plans, {Members} members, {Memberships} memberships, {Instructors} instructors, {ClassTypes} class types, {Schedules} schedules, {Bookings} bookings",
            3, members.Length, memberships.Length, instructors.Length, 6, schedules.Length, bookings.Count);
    }
}
