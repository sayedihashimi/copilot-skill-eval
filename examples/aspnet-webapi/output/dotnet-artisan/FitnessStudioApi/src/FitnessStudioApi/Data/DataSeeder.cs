using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basic = new MembershipPlan
        {
            Id = 1, Name = "Basic", Description = "Access to standard classes, up to 3 per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now
        };
        var premium = new MembershipPlan
        {
            Id = 2, Name = "Premium", Description = "Access to all classes including premium, up to 5 per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        var elite = new MembershipPlan
        {
            Id = 3, Name = "Elite", Description = "Unlimited access to all classes including premium",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        db.MembershipPlans.AddRange(basic, premium, elite);

        // Members
        var members = new[]
        {
            new Member { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 2, FirstName = "Carlos", LastName = "Rivera", Email = "carlos.rivera@example.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Maria Rivera", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 3, FirstName = "Diana", LastName = "Chen", Email = "diana.chen@example.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Wei Chen", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 4, FirstName = "Ethan", LastName = "Williams", Email = "ethan.williams@example.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 5, FirstName = "Fatima", LastName = "Ali", Email = "fatima.ali@example.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Hassan Ali", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 6, FirstName = "George", LastName = "Patel", Email = "george.patel@example.com", Phone = "555-0601", DateOfBirth = new DateOnly(2000, 9, 25), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 7, FirstName = "Hannah", LastName = "Kim", Email = "hannah.kim@example.com", Phone = "555-0701", DateOfBirth = new DateOnly(1993, 2, 14), EmergencyContactName = "Jin Kim", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new Member { Id = 8, FirstName = "Ivan", LastName = "Novak", Email = "ivan.novak@example.com", Phone = "555-0801", DateOfBirth = new DateOnly(1998, 12, 3), EmergencyContactName = "Petra Novak", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now },
        };
        db.Members.AddRange(members);

        // Memberships
        var memberships = new[]
        {
            new Membership { Id = 1, MemberId = 1, MembershipPlanId = 2, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 2, MemberId = 2, MembershipPlanId = 3, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = today.AddDays(-25), EndDate = today.AddDays(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = today.AddDays(-8), EndDate = today.AddDays(22), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7
            new Membership { Id = 7, MemberId = 7, MembershipPlanId = 1, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Cancelled membership for member 8
            new Membership { Id = 8, MemberId = 8, MembershipPlanId = 2, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now },
        };
        db.Memberships.AddRange(memberships);

        // Instructors
        var instructors = new[]
        {
            new Instructor { Id = 1, FirstName = "Sarah", LastName = "Martinez", Email = "sarah.m@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga and Pilates instructor with 10 years of experience.", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 2, FirstName = "Marcus", LastName = "Thompson", Email = "marcus.t@zenithfitness.com", Phone = "555-1002", Bio = "Former competitive cyclist and HIIT specialist.", Specializations = "HIIT, Spin, Boxing", HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 3, FirstName = "Yuki", LastName = "Tanaka", Email = "yuki.t@zenithfitness.com", Phone = "555-1003", Bio = "Mindfulness and meditation expert, registered yoga teacher.", Specializations = "Yoga, Meditation", HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 4, FirstName = "James", LastName = "O'Brien", Email = "james.o@zenithfitness.com", Phone = "555-1004", Bio = "Professional boxing coach and strength trainer.", Specializations = "Boxing, HIIT", HireDate = new DateOnly(2022, 8, 20), CreatedAt = now, UpdatedAt = now },
        };
        db.Instructors.AddRange(instructors);

        // Class Types
        var classTypes = new[]
        {
            new ClassType { Id = 1, Name = "Yoga", Description = "Vinyasa flow yoga for all levels. Improve flexibility and mindfulness.", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 2, Name = "HIIT", Description = "High-Intensity Interval Training. Maximum calorie burn in minimum time.", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 3, Name = "Spin", Description = "Indoor cycling with energizing music and varied intensity.", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 4, Name = "Pilates", Description = "Core-strengthening Pilates with reformer equipment.", DefaultDurationMinutes = 55, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 5, Name = "Boxing", Description = "Boxing fitness combining technique drills and cardio.", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 6, Name = "Meditation", Description = "Guided meditation and breathing exercises for stress relief.", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
        };
        db.ClassTypes.AddRange(classTypes);

        // Class Schedules (next 7 days)
        var tomorrow = now.Date.AddDays(1);
        var schedules = new[]
        {
            new ClassSchedule { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 5, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 3, ClassTypeId = 3, InstructorId = 2, StartTime = tomorrow.AddHours(17), EndTime = tomorrow.AddHours(17).AddMinutes(45), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 4, ClassTypeId = 4, InstructorId = 1, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(10).AddMinutes(55), Capacity = 10, CurrentEnrollment = 4, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 5, ClassTypeId = 5, InstructorId = 4, StartTime = tomorrow.AddDays(1).AddHours(18), EndTime = tomorrow.AddDays(1).AddHours(19), Capacity = 15, CurrentEnrollment = 8, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 6, ClassTypeId = 6, InstructorId = 3, StartTime = tomorrow.AddDays(2).AddHours(6), EndTime = tomorrow.AddDays(2).AddHours(6).AddMinutes(30), Capacity = 25, CurrentEnrollment = 2, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 7, ClassTypeId = 1, InstructorId = 3, StartTime = tomorrow.AddDays(2).AddHours(12), EndTime = tomorrow.AddDays(2).AddHours(13), Capacity = 20, CurrentEnrollment = 10, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 8, ClassTypeId = 2, InstructorId = 4, StartTime = tomorrow.AddDays(3).AddHours(7), EndTime = tomorrow.AddDays(3).AddHours(7).AddMinutes(45), Capacity = 15, CurrentEnrollment = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 9, ClassTypeId = 3, InstructorId = 2, StartTime = tomorrow.AddDays(3).AddHours(18), EndTime = tomorrow.AddDays(3).AddHours(18).AddMinutes(45), Capacity = 12, CurrentEnrollment = 6, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 10, ClassTypeId = 5, InstructorId = 4, StartTime = tomorrow.AddDays(4).AddHours(17), EndTime = tomorrow.AddDays(4).AddHours(18), Capacity = 15, CurrentEnrollment = 3, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 11, ClassTypeId = 6, InstructorId = 3, StartTime = tomorrow.AddDays(5).AddHours(8), EndTime = tomorrow.AddDays(5).AddHours(8).AddMinutes(30), Capacity = 25, CurrentEnrollment = 5, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new ClassSchedule { Id = 12, ClassTypeId = 1, InstructorId = 1, StartTime = tomorrow.AddDays(4).AddHours(7), EndTime = tomorrow.AddDays(4).AddHours(8), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
        };
        db.ClassSchedules.AddRange(schedules);

        // Bookings
        var bookings = new List<Booking>
        {
            // Class 1 (Yoga tomorrow) - 3 confirmed
            new() { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, ClassScheduleId = 1, MemberId = 3, BookingDate = now.AddHours(-5), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Class 2 (HIIT tomorrow) - 5 confirmed
            new() { Id = 4, ClassScheduleId = 2, MemberId = 1, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, ClassScheduleId = 2, MemberId = 4, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, ClassScheduleId = 2, MemberId = 5, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, ClassScheduleId = 2, MemberId = 2, BookingDate = now.AddHours(-8), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, ClassScheduleId = 2, MemberId = 6, BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Class 3 (Spin - FULL) - 12 confirmed + 2 waitlisted (we'll show a few)
            new() { Id = 9, ClassScheduleId = 3, MemberId = 1, BookingDate = now.AddDays(-3), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, ClassScheduleId = 3, MemberId = 2, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, ClassScheduleId = 3, MemberId = 4, BookingDate = now.AddDays(-1), Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, ClassScheduleId = 3, MemberId = 5, BookingDate = now.AddHours(-6), Status = BookingStatus.Waitlisted, WaitlistPosition = 2, CreatedAt = now, UpdatedAt = now },

            // A cancelled booking
            new() { Id = 13, ClassScheduleId = 4, MemberId = 3, BookingDate = now.AddDays(-2), Status = BookingStatus.Cancelled, CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // An attended booking (past-ish, but for demo)
            new() { Id = 14, ClassScheduleId = 7, MemberId = 2, BookingDate = now.AddHours(-2), Status = BookingStatus.Attended, CheckInTime = now.AddHours(-1), CreatedAt = now, UpdatedAt = now },

            // A no-show
            new() { Id = 15, ClassScheduleId = 5, MemberId = 6, BookingDate = now.AddDays(-1), Status = BookingStatus.NoShow, CreatedAt = now, UpdatedAt = now },
        };
        db.Bookings.AddRange(bookings);

        await db.SaveChangesAsync();
    }
}
