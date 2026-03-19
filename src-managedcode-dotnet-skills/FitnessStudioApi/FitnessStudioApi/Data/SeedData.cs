using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return;

        var plans = CreatePlans();
        db.MembershipPlans.AddRange(plans);

        var members = CreateMembers();
        db.Members.AddRange(members);

        var instructors = CreateInstructors();
        db.Instructors.AddRange(instructors);

        var classTypes = CreateClassTypes();
        db.ClassTypes.AddRange(classTypes);

        await db.SaveChangesAsync();

        var memberships = CreateMemberships(members, plans);
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        var schedules = CreateClassSchedules(classTypes, instructors);
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        var bookings = CreateBookings(schedules, members);
        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();

        // Update enrollment counts
        foreach (var schedule in schedules)
        {
            schedule.CurrentEnrollment = bookings.Count(b =>
                b.ClassScheduleId == schedule.Id &&
                b.Status is BookingStatus.Confirmed or BookingStatus.Attended);
            schedule.WaitlistCount = bookings.Count(b =>
                b.ClassScheduleId == schedule.Id &&
                b.Status == BookingStatus.Waitlisted);
        }
        await db.SaveChangesAsync();
    }

    private static List<MembershipPlan> CreatePlans() =>
    [
        new()
        {
            Id = 1, Name = "Basic", Description = "Access to standard classes, 3 bookings per week",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false
        },
        new()
        {
            Id = 2, Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week",
            DurationMonths = 3, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true
        },
        new()
        {
            Id = 3, Name = "Elite", Description = "Unlimited access to all classes and premium perks",
            DurationMonths = 12, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true
        }
    ];

    private static List<Member> CreateMembers() =>
    [
        new()
        {
            Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com",
            Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15),
            EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102"
        },
        new()
        {
            Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com",
            Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22),
            EmergencyContactName = "Lin Chen", EmergencyContactPhone = "555-0202"
        },
        new()
        {
            Id = 3, FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com",
            Phone = "555-0301", DateOfBirth = new DateOnly(1995, 11, 8),
            EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0302"
        },
        new()
        {
            Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com",
            Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30),
            EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0402"
        },
        new()
        {
            Id = 5, FirstName = "Priya", LastName = "Patel", Email = "priya.patel@email.com",
            Phone = "555-0501", DateOfBirth = new DateOnly(1992, 5, 14),
            EmergencyContactName = "Raj Patel", EmergencyContactPhone = "555-0502"
        },
        new()
        {
            Id = 6, FirstName = "David", LastName = "Kim", Email = "david.kim@email.com",
            Phone = "555-0601", DateOfBirth = new DateOnly(2000, 9, 3),
            EmergencyContactName = "Min Kim", EmergencyContactPhone = "555-0602"
        },
        new()
        {
            Id = 7, FirstName = "Emma", LastName = "Taylor", Email = "emma.taylor@email.com",
            Phone = "555-0701", DateOfBirth = new DateOnly(1997, 12, 25),
            EmergencyContactName = "John Taylor", EmergencyContactPhone = "555-0702"
        },
        new()
        {
            Id = 8, FirstName = "Omar", LastName = "Hassan", Email = "omar.hassan@email.com",
            Phone = "555-0801", DateOfBirth = new DateOnly(1993, 4, 18),
            EmergencyContactName = "Fatima Hassan", EmergencyContactPhone = "555-0802"
        }
    ];

    private static List<Instructor> CreateInstructors() =>
    [
        new()
        {
            Id = 1, FirstName = "Sarah", LastName = "Mitchell", Email = "sarah.mitchell@zenith.com",
            Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience",
            Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2020, 1, 15)
        },
        new()
        {
            Id = 2, FirstName = "Jake", LastName = "Torres", Email = "jake.torres@zenith.com",
            Phone = "555-1002", Bio = "Former professional boxer and HIIT specialist",
            Specializations = "HIIT,Boxing,Strength", HireDate = new DateOnly(2021, 3, 1)
        },
        new()
        {
            Id = 3, FirstName = "Luna", LastName = "Park", Email = "luna.park@zenith.com",
            Phone = "555-1003", Bio = "Spin and cardio expert, marathon runner",
            Specializations = "Spin,Cardio,HIIT", HireDate = new DateOnly(2022, 6, 10)
        },
        new()
        {
            Id = 4, FirstName = "Ryan", LastName = "Brooks", Email = "ryan.brooks@zenith.com",
            Phone = "555-1004", Bio = "Pilates and functional movement specialist",
            Specializations = "Pilates,Yoga,Stretching", HireDate = new DateOnly(2023, 2, 20)
        }
    ];

    private static List<ClassType> CreateClassTypes() =>
    [
        new()
        {
            Id = 1, Name = "Yoga", Description = "Vinyasa flow yoga for all levels",
            DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false,
            CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels
        },
        new()
        {
            Id = 2, Name = "HIIT", Description = "High-intensity interval training to torch calories",
            DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false,
            CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate
        },
        new()
        {
            Id = 3, Name = "Spin", Description = "Indoor cycling with energizing music",
            DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = true,
            CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate
        },
        new()
        {
            Id = 4, Name = "Pilates", Description = "Core-strengthening reformer Pilates",
            DefaultDurationMinutes = 55, DefaultCapacity = 10, IsPremium = true,
            CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner
        },
        new()
        {
            Id = 5, Name = "Boxing", Description = "Cardio boxing with bag work and technique drills",
            DefaultDurationMinutes = 60, DefaultCapacity = 16, IsPremium = false,
            CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced
        },
        new()
        {
            Id = 6, Name = "Meditation", Description = "Guided mindfulness and breathing session",
            DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false,
            CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner
        }
    ];

    private static List<Membership> CreateMemberships(List<Member> members, List<MembershipPlan> plans)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return
        [
            new()
            {
                Id = 1, MemberId = 1, MembershipPlanId = 3,
                StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 2, MemberId = 2, MembershipPlanId = 2,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 3, MemberId = 3, MembershipPlanId = 1,
                StartDate = today, EndDate = today.AddMonths(1),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 4, MemberId = 4, MembershipPlanId = 2,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 5, MemberId = 5, MembershipPlanId = 3,
                StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 6, MemberId = 6, MembershipPlanId = 1,
                StartDate = today.AddDays(-10), EndDate = today.AddDays(20),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 7, MemberId = 7, MembershipPlanId = 2,
                StartDate = today.AddMonths(-4), EndDate = today.AddMonths(-1),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid
            },
            new()
            {
                Id = 8, MemberId = 8, MembershipPlanId = 1,
                StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1),
                Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded
            }
        ];
    }

    private static List<ClassSchedule> CreateClassSchedules(
        List<ClassType> classTypes, List<Instructor> instructors)
    {
        var today = DateTime.UtcNow.Date;
        return
        [
            // Day 1 — tomorrow
            new()
            {
                Id = 1, ClassTypeId = 1, InstructorId = 1,
                StartTime = today.AddDays(1).AddHours(7), EndTime = today.AddDays(1).AddHours(8),
                Capacity = 20, Room = "Studio A", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 2, ClassTypeId = 2, InstructorId = 2,
                StartTime = today.AddDays(1).AddHours(9), EndTime = today.AddDays(1).AddHours(9).AddMinutes(45),
                Capacity = 15, Room = "Studio B", Status = ClassScheduleStatus.Scheduled
            },
            // Day 2
            new()
            {
                Id = 3, ClassTypeId = 3, InstructorId = 3,
                StartTime = today.AddDays(2).AddHours(6), EndTime = today.AddDays(2).AddHours(6).AddMinutes(45),
                Capacity = 12, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 4, ClassTypeId = 4, InstructorId = 4,
                StartTime = today.AddDays(2).AddHours(10), EndTime = today.AddDays(2).AddHours(10).AddMinutes(55),
                Capacity = 10, Room = "Studio C", Status = ClassScheduleStatus.Scheduled
            },
            // Day 3
            new()
            {
                Id = 5, ClassTypeId = 5, InstructorId = 2,
                StartTime = today.AddDays(3).AddHours(8), EndTime = today.AddDays(3).AddHours(9),
                Capacity = 16, Room = "Boxing Ring", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 6, ClassTypeId = 6, InstructorId = 1,
                StartTime = today.AddDays(3).AddHours(12), EndTime = today.AddDays(3).AddHours(12).AddMinutes(30),
                Capacity = 25, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled
            },
            // Day 4
            new()
            {
                Id = 7, ClassTypeId = 1, InstructorId = 4,
                StartTime = today.AddDays(4).AddHours(7), EndTime = today.AddDays(4).AddHours(8),
                Capacity = 20, Room = "Studio A", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 8, ClassTypeId = 2, InstructorId = 2,
                StartTime = today.AddDays(4).AddHours(17), EndTime = today.AddDays(4).AddHours(17).AddMinutes(45),
                Capacity = 15, Room = "Studio B", Status = ClassScheduleStatus.Scheduled
            },
            // Day 5
            new()
            {
                Id = 9, ClassTypeId = 3, InstructorId = 3,
                StartTime = today.AddDays(5).AddHours(6), EndTime = today.AddDays(5).AddHours(6).AddMinutes(45),
                Capacity = 3, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 10, ClassTypeId = 5, InstructorId = 2,
                StartTime = today.AddDays(5).AddHours(18), EndTime = today.AddDays(5).AddHours(19),
                Capacity = 16, Room = "Boxing Ring", Status = ClassScheduleStatus.Scheduled
            },
            // Day 6
            new()
            {
                Id = 11, ClassTypeId = 4, InstructorId = 4,
                StartTime = today.AddDays(6).AddHours(9), EndTime = today.AddDays(6).AddHours(9).AddMinutes(55),
                Capacity = 10, Room = "Studio C", Status = ClassScheduleStatus.Scheduled
            },
            new()
            {
                Id = 12, ClassTypeId = 6, InstructorId = 1,
                StartTime = today.AddDays(6).AddHours(11), EndTime = today.AddDays(6).AddHours(11).AddMinutes(30),
                Capacity = 25, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled
            }
        ];
    }

    private static List<Booking> CreateBookings(
        List<ClassSchedule> schedules, List<Member> members)
    {
        var now = DateTime.UtcNow;
        return
        [
            // Class 1 (Yoga) — several confirmed
            new() { Id = 1, ClassScheduleId = 1, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 2, ClassScheduleId = 1, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 3, ClassScheduleId = 1, MemberId = 3, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12) },

            // Class 2 (HIIT) — confirmed
            new() { Id = 4, ClassScheduleId = 2, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 5, ClassScheduleId = 2, MemberId = 4, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6) },

            // Class 3 (Spin — premium) — confirmed for premium/elite members
            new() { Id = 6, ClassScheduleId = 3, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2) },
            new() { Id = 7, ClassScheduleId = 3, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },

            // Class 4 (Pilates — premium)
            new() { Id = 8, ClassScheduleId = 4, MemberId = 5, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },

            // Class 5 (Boxing)
            new() { Id = 9, ClassScheduleId = 5, MemberId = 4, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 10, ClassScheduleId = 5, MemberId = 6, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-3) },

            // Class 9 (Spin — capacity 3) — full with waitlist
            new() { Id = 11, ClassScheduleId = 9, MemberId = 1, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2) },
            new() { Id = 12, ClassScheduleId = 9, MemberId = 2, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2) },
            new() { Id = 13, ClassScheduleId = 9, MemberId = 5, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1) },
            new() { Id = 14, ClassScheduleId = 9, MemberId = 4, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-6) },
            new() { Id = 15, ClassScheduleId = 9, MemberId = 6, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-3) },

            // A cancelled booking
            new()
            {
                Id = 16, ClassScheduleId = 6, MemberId = 3, Status = BookingStatus.Cancelled,
                BookingDate = now.AddDays(-2), CancellationDate = now.AddDays(-1),
                CancellationReason = "Schedule conflict"
            }
        ];
    }
}
