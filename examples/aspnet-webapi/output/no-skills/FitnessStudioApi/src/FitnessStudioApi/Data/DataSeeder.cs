using FitnessStudioApi.Data;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class DataSeeder
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(FitnessDbContext db, ILogger<DataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_db.MembershipPlans.Any())
        {
            _logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding database with demo data...");

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // --- Membership Plans ---
        var basicPlan = new MembershipPlan { Name = "Basic", Description = "Essential access with limited class bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now };
        var premiumPlan = new MembershipPlan { Name = "Premium", Description = "Enhanced access with more classes and premium class access", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        var elitePlan = new MembershipPlan { Name = "Elite", Description = "Unlimited access to all classes including premium", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };

        _db.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        await _db.SaveChangesAsync();

        // --- Members ---
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Ben", LastName = "Williams", Email = "ben.williams@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Clara", LastName = "Davis", Email = "clara.davis@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Mike Davis", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "David", LastName = "Martinez", Email = "david.martinez@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Lisa Martinez", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Emma", LastName = "Garcia", Email = "emma.garcia@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Carlos Garcia", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Frank", LastName = "Brown", Email = "frank.brown@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1980, 9, 25), EmergencyContactName = "Jane Brown", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-10), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Grace", LastName = "Wilson", Email = "grace.wilson@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1998, 2, 14), EmergencyContactName = "Tom Wilson", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Henry", LastName = "Taylor", Email = "henry.taylor@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1993, 6, 3), EmergencyContactName = "Sophia Taylor", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
        };

        _db.Members.AddRange(members);
        await _db.SaveChangesAsync();

        // --- Memberships ---
        var memberships = new[]
        {
            // Active memberships
            new Membership { MemberId = members[0].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[1].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[4].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddDays(-3), EndDate = today.AddDays(27), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddDays(-25), EndDate = today.AddDays(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7 (Grace)
            new Membership { MemberId = members[6].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 8 (Henry)
            new Membership { MemberId = members[7].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
        };

        _db.Memberships.AddRange(memberships);
        await _db.SaveChangesAsync();

        // --- Instructors ---
        var instructors = new[]
        {
            new Instructor { FirstName = "Maya", LastName = "Patel", Email = "maya.patel@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience in Vinyasa and Hatha yoga.", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Jake", LastName = "Thompson", Email = "jake.thompson@zenithfitness.com", Phone = "555-1002", Bio = "Former professional athlete specializing in high-intensity interval training and boxing.", Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Sophia", LastName = "Chen", Email = "sophia.chen@zenithfitness.com", Phone = "555-1003", Bio = "Dance-trained Pilates instructor focused on core strength and flexibility.", Specializations = "Pilates, Yoga, Meditation", HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Marcus", LastName = "Lee", Email = "marcus.lee@zenithfitness.com", Phone = "555-1004", Bio = "CrossFit Level 2 trainer passionate about functional fitness and endurance.", Specializations = "HIIT, Spin, Boxing", HireDate = new DateOnly(2022, 9, 20), CreatedAt = now, UpdatedAt = now },
        };

        _db.Instructors.AddRange(instructors);
        await _db.SaveChangesAsync();

        // --- Class Types ---
        var classTypes = new[]
        {
            new ClassType { Name = "Yoga", Description = "Flow through postures to improve flexibility, balance, and mental clarity", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn and cardiovascular fitness", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Spin", Description = "Indoor cycling class set to motivating music with varying intensity levels", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Pilates", Description = "Core-focused exercises for strength, flexibility, and body awareness", DefaultDurationMinutes = 55, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Boxing", Description = "Premium boxing class with personal technique coaching and sparring", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Meditation", Description = "Premium guided meditation and mindfulness session for deep relaxation", DefaultDurationMinutes = 30, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
        };

        _db.ClassTypes.AddRange(classTypes);
        await _db.SaveChangesAsync();

        // --- Class Schedules (next 7 days) ---
        var tomorrow = now.Date.AddDays(1);
        var schedules = new[]
        {
            // Tomorrow
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9.75), Capacity = 15, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddHours(11), EndTime = tomorrow.AddHours(11.75), Capacity = 20, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day after tomorrow
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(8.92), Capacity = 15, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11), Capacity = 10, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 3 - this one will be at capacity
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(7.5), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 1, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(2).AddHours(17), EndTime = tomorrow.AddDays(2).AddHours(18), Capacity = 20, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day 4
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(3).AddHours(6), EndTime = tomorrow.AddDays(3).AddHours(6.75), Capacity = 15, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(3).AddHours(18), EndTime = tomorrow.AddDays(3).AddHours(18.75), Capacity = 20, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day 5
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(4).AddHours(9), EndTime = tomorrow.AddDays(4).AddHours(9.92), Capacity = 15, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(4).AddHours(16), EndTime = tomorrow.AddDays(4).AddHours(17), Capacity = 10, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 6 - cancelled class
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(5).AddHours(7), EndTime = tomorrow.AddDays(5).AddHours(8), Capacity = 20, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
        };

        _db.ClassSchedules.AddRange(schedules);
        await _db.SaveChangesAsync();

        // --- Bookings ---
        var bookings = new List<Booking>
        {
            // Confirmed bookings for tomorrow's Yoga (schedule 0)
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },

            // Confirmed bookings for tomorrow's HIIT (schedule 1)
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },

            // Bookings for the full Meditation class (schedule 5, capacity 3)
            new Booking { ClassScheduleId = schedules[5].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[5].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1).AddHours(1), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[5].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1).AddHours(2), CreatedAt = now, UpdatedAt = now },
            // Waitlisted for the full class
            new Booking { ClassScheduleId = schedules[5].Id, MemberId = members[3].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddDays(-1).AddHours(3), CreatedAt = now, UpdatedAt = now },

            // Cancelled booking
            new Booking { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id, Status = BookingStatus.Cancelled, BookingDate = now.AddHours(-5), CancellationDate = now.AddHours(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // Confirmed for Spin (schedule 2)
            new Booking { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },

            // Bookings for Pilates (schedule 3)
            new Booking { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[3].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },

            // Booking for Boxing (schedule 4, premium class)
            new Booking { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },
            new Booking { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now, CreatedAt = now, UpdatedAt = now },
        };

        _db.Bookings.AddRange(bookings);

        // Update enrollment counts
        schedules[0].CurrentEnrollment = 3;
        schedules[1].CurrentEnrollment = 2;
        // schedule[5] already set above
        schedules[2].CurrentEnrollment = 1;
        schedules[3].CurrentEnrollment = 2;
        schedules[4].CurrentEnrollment = 2;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Database seeded successfully with {Plans} plans, {Members} members, {Instructors} instructors, {ClassTypes} class types, {Schedules} schedules, {Bookings} bookings",
            3, members.Length, instructors.Length, classTypes.Length, schedules.Length, bookings.Count);
    }
}
