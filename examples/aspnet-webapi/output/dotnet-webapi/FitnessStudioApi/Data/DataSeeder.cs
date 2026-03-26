using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class DataSeeder(FitnessDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.MembershipPlans.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Membership Plans
        var basicPlan = new MembershipPlan
        {
            Id = 1, Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, IsActive = true
        };
        var premiumPlan = new MembershipPlan
        {
            Id = 2, Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, IsActive = true
        };
        var elitePlan = new MembershipPlan
        {
            Id = 3, Name = "Elite", Description = "Unlimited access to all classes and premium features",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, IsActive = true
        };
        context.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);

        // Members
        var members = new List<Member>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101",
                     DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6) },
            new() { Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201",
                     DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0202", JoinDate = today.AddMonths(-4) },
            new() { Id = 3, FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com", Phone = "555-0301",
                     DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0302", JoinDate = today.AddMonths(-3) },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0401",
                     DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0402", JoinDate = today.AddMonths(-8) },
            new() { Id = 5, FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0501",
                     DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Michael Davis", EmergencyContactPhone = "555-0502", JoinDate = today.AddMonths(-2) },
            new() { Id = 6, FirstName = "Liam", LastName = "Brown", Email = "liam.brown@email.com", Phone = "555-0601",
                     DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Olivia Brown", EmergencyContactPhone = "555-0602", JoinDate = today.AddMonths(-5) },
            new() { Id = 7, FirstName = "Ava", LastName = "Martinez", Email = "ava.martinez@email.com", Phone = "555-0701",
                     DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "David Martinez", EmergencyContactPhone = "555-0702", JoinDate = today.AddMonths(-1) },
            new() { Id = 8, FirstName = "Noah", LastName = "Taylor", Email = "noah.taylor@email.com", Phone = "555-0801",
                     DateOfBirth = new DateOnly(1987, 6, 18), EmergencyContactName = "Emily Taylor", EmergencyContactPhone = "555-0802", JoinDate = today.AddMonths(-7) }
        };
        context.Members.AddRange(members);

        // Memberships (6 active, 2 expired)
        var memberships = new List<Membership>
        {
            new() { Id = 1, MemberId = 1, MembershipPlanId = 2, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 2, MemberId = 2, MembershipPlanId = 3, StartDate = today.AddDays(-10), EndDate = today.AddDays(20), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = today.AddDays(-5), EndDate = today.AddDays(25), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = today.AddDays(-25), EndDate = today.AddDays(5), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = today.AddDays(-8), EndDate = today.AddDays(22), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 7, MemberId = 7, MembershipPlanId = 2, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid },
            new() { Id = 8, MemberId = 8, MembershipPlanId = 1, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid }
        };
        context.Memberships.AddRange(memberships);

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Mitchell", Email = "sarah.mitchell@zenith.com", Phone = "555-1001",
                     Bio = "Certified yoga and pilates instructor with 10 years experience.", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15) },
            new() { Id = 2, FirstName = "Derek", LastName = "Thompson", Email = "derek.thompson@zenith.com", Phone = "555-1002",
                     Bio = "Former competitive athlete specializing in high-intensity training.", Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2019, 6, 1) },
            new() { Id = 3, FirstName = "Maya", LastName = "Patel", Email = "maya.patel@zenith.com", Phone = "555-1003",
                     Bio = "Holistic wellness coach focused on mind-body connection.", Specializations = "Yoga, Meditation", HireDate = new DateOnly(2021, 3, 10) },
            new() { Id = 4, FirstName = "Ryan", LastName = "Foster", Email = "ryan.foster@zenith.com", Phone = "555-1004",
                     Bio = "Cycling enthusiast and certified spin instructor.", Specializations = "Spin, HIIT", HireDate = new DateOnly(2022, 8, 20) }
        };
        context.Instructors.AddRange(instructors);

        // Class Types (Boxing and HIIT are premium)
        var classTypes = new List<ClassType>
        {
            new() { Id = 1, Name = "Yoga", Description = "Traditional yoga focusing on flexibility and mindfulness", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels },
            new() { Id = 2, Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced },
            new() { Id = 3, Name = "Spin", Description = "Indoor cycling class with energizing music", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 400, DifficultyLevel = DifficultyLevel.Intermediate },
            new() { Id = 4, Name = "Pilates", Description = "Core-strengthening exercises for posture and flexibility", DefaultDurationMinutes = 50, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner },
            new() { Id = 5, Name = "Boxing", Description = "Boxing-inspired fitness class for strength and cardio", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced },
            new() { Id = 6, Name = "Meditation", Description = "Guided meditation for stress relief and mental clarity", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner }
        };
        context.ClassTypes.AddRange(classTypes);

        // Class Schedules - 12+ over next 7 days
        var tomorrow = DateTime.Today.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Tomorrow
            new() { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 5, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 3, ClassTypeId = 3, InstructorId = 4, StartTime = tomorrow.AddHours(11), EndTime = tomorrow.AddHours(11).AddMinutes(45), Capacity = 20, CurrentEnrollment = 10, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            // Day after tomorrow
            new() { Id = 4, ClassTypeId = 4, InstructorId = 1, StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(8).AddMinutes(50), Capacity = 15, CurrentEnrollment = 8, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 5, ClassTypeId = 5, InstructorId = 2, StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11), Capacity = 12, CurrentEnrollment = 6, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 6, ClassTypeId = 6, InstructorId = 3, StartTime = tomorrow.AddDays(1).AddHours(12), EndTime = tomorrow.AddDays(1).AddHours(12).AddMinutes(30), Capacity = 25, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 3
            new() { Id = 7, ClassTypeId = 1, InstructorId = 3, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8), Capacity = 20, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 8, ClassTypeId = 3, InstructorId = 4, StartTime = tomorrow.AddDays(2).AddHours(17), EndTime = tomorrow.AddDays(2).AddHours(17).AddMinutes(45), Capacity = 20, CurrentEnrollment = 12, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled },
            // Day 4
            new() { Id = 9, ClassTypeId = 2, InstructorId = 2, StartTime = tomorrow.AddDays(3).AddHours(6), EndTime = tomorrow.AddDays(3).AddHours(6).AddMinutes(45), Capacity = 15, CurrentEnrollment = 4, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            new() { Id = 10, ClassTypeId = 4, InstructorId = 1, StartTime = tomorrow.AddDays(3).AddHours(9), EndTime = tomorrow.AddDays(3).AddHours(9).AddMinutes(50), Capacity = 15, CurrentEnrollment = 7, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled },
            // Day 5
            new() { Id = 11, ClassTypeId = 5, InstructorId = 2, StartTime = tomorrow.AddDays(4).AddHours(10), EndTime = tomorrow.AddDays(4).AddHours(11), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled },
            // Cancelled class
            new() { Id = 12, ClassTypeId = 6, InstructorId = 3, StartTime = tomorrow.AddDays(4).AddHours(14), EndTime = tomorrow.AddDays(4).AddHours(14).AddMinutes(30), Capacity = 25, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable" },
            // Past class (completed)
            new() { Id = 13, ClassTypeId = 1, InstructorId = 1, StartTime = DateTime.Today.AddHours(7), EndTime = DateTime.Today.AddHours(8), Capacity = 20, CurrentEnrollment = 8, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Completed }
        };
        context.ClassSchedules.AddRange(schedules);

        // Bookings - 15+ in various states
        var bookings = new List<Booking>
        {
            // Confirmed bookings for tomorrow's yoga (schedule 1)
            new() { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed },
            new() { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed },
            new() { Id = 3, ClassScheduleId = 1, MemberId = 4, BookingDate = now.AddHours(-12), Status = BookingStatus.Confirmed },
            new() { Id = 4, ClassScheduleId = 1, MemberId = 5, BookingDate = now.AddHours(-6), Status = BookingStatus.Confirmed },
            new() { Id = 5, ClassScheduleId = 1, MemberId = 6, BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed },

            // HIIT class (schedule 2) - full with waitlist
            new() { Id = 6, ClassScheduleId = 2, MemberId = 1, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed },
            new() { Id = 7, ClassScheduleId = 2, MemberId = 2, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed },
            new() { Id = 8, ClassScheduleId = 2, MemberId = 4, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed },
            new() { Id = 9, ClassScheduleId = 2, MemberId = 6, BookingDate = now.AddHours(-12), Status = BookingStatus.Waitlisted, WaitlistPosition = 1 },
            new() { Id = 10, ClassScheduleId = 2, MemberId = 2, BookingDate = now.AddHours(-6), Status = BookingStatus.Waitlisted, WaitlistPosition = 2 },

            // Cancelled booking
            new() { Id = 11, ClassScheduleId = 3, MemberId = 3, BookingDate = now.AddDays(-1), Status = BookingStatus.Cancelled, CancellationDate = now.AddHours(-5), CancellationReason = "Schedule conflict" },

            // Spin class bookings
            new() { Id = 12, ClassScheduleId = 3, MemberId = 1, BookingDate = now.AddHours(-10), Status = BookingStatus.Confirmed },
            new() { Id = 13, ClassScheduleId = 3, MemberId = 5, BookingDate = now.AddHours(-8), Status = BookingStatus.Confirmed },

            // Past class bookings - attended and no-show
            new() { Id = 14, ClassScheduleId = 13, MemberId = 1, BookingDate = now.AddDays(-1), Status = BookingStatus.Attended, CheckInTime = DateTime.Today.AddHours(6).AddMinutes(50) },
            new() { Id = 15, ClassScheduleId = 13, MemberId = 3, BookingDate = now.AddDays(-1), Status = BookingStatus.NoShow },

            // Future bookings
            new() { Id = 16, ClassScheduleId = 4, MemberId = 2, BookingDate = now.AddHours(-4), Status = BookingStatus.Confirmed },
            new() { Id = 17, ClassScheduleId = 5, MemberId = 4, BookingDate = now.AddHours(-3), Status = BookingStatus.Confirmed },
        };
        context.Bookings.AddRange(bookings);

        await context.SaveChangesAsync();
    }
}
