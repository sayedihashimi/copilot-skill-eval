using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public class FitnessDbContext : DbContext
{
    public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }

    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<ClassType> ClassTypes => Set<ClassType>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureMembershipPlan(modelBuilder);
        ConfigureMember(modelBuilder);
        ConfigureMembership(modelBuilder);
        ConfigureInstructor(modelBuilder);
        ConfigureClassType(modelBuilder);
        ConfigureClassSchedule(modelBuilder);
        ConfigureBooking(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureMembershipPlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });
    }

    private static void ConfigureMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void ConfigureMembership(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasConversion<string>();

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MembershipPlan)
                .WithMany()
                .HasForeignKey(e => e.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInstructor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void ConfigureClassType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.DifficultyLevel).HasConversion<string>();
        });
    }

    private static void ConfigureClassSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(e => e.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBooking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(e => e.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ClassScheduleId, e.MemberId });
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Membership Plans
        modelBuilder.Entity<MembershipPlan>().HasData(
            new MembershipPlan { Id = 1, Name = "Basic", Description = "Access to standard classes with limited bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 2, Name = "Premium", Description = "Access to all classes including premium with more bookings per week", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 3, Name = "Elite", Description = "Unlimited access to all classes and premium amenities", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Members
        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = new DateOnly(2024, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lisa Chen", EmergencyContactPhone = "555-0202", JoinDate = new DateOnly(2024, 8, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 3, FirstName = "Sophia", LastName = "Williams", Email = "sophia.williams@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "James Williams", EmergencyContactPhone = "555-0302", JoinDate = new DateOnly(2024, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 4, FirstName = "Daniel", LastName = "Garcia", Email = "daniel.garcia@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 5, 30), EmergencyContactName = "Maria Garcia", EmergencyContactPhone = "555-0402", JoinDate = new DateOnly(2024, 7, 10), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 5, FirstName = "Emma", LastName = "Brown", Email = "emma.brown@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 1, 20), EmergencyContactName = "Tom Brown", EmergencyContactPhone = "555-0502", JoinDate = new DateOnly(2024, 10, 5), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 6, FirstName = "Liam", LastName = "Taylor", Email = "liam.taylor@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1993, 9, 12), EmergencyContactName = "Sarah Taylor", EmergencyContactPhone = "555-0602", JoinDate = new DateOnly(2024, 5, 20), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 7, FirstName = "Olivia", LastName = "Martinez", Email = "olivia.martinez@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1991, 4, 3), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0702", JoinDate = new DateOnly(2024, 11, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 8, FirstName = "Noah", LastName = "Anderson", Email = "noah.anderson@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 12, 25), EmergencyContactName = "Karen Anderson", EmergencyContactPhone = "555-0802", JoinDate = new DateOnly(2024, 4, 15), IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // Instructors
        modelBuilder.Entity<Instructor>().HasData(
            new Instructor { Id = 1, FirstName = "Sarah", LastName = "Mitchell", Email = "sarah.mitchell@zenith.com", Phone = "555-1001", Bio = "Certified yoga instructor with 10 years of experience", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2022, 1, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 2, FirstName = "Jake", LastName = "Rivera", Email = "jake.rivera@zenith.com", Phone = "555-1002", Bio = "Former competitive athlete specializing in high-intensity training", Specializations = "HIIT,Boxing,Spin", HireDate = new DateOnly(2022, 3, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 3, FirstName = "Emily", LastName = "Park", Email = "emily.park@zenith.com", Phone = "555-1003", Bio = "Pilates and barre expert with a background in dance", Specializations = "Pilates,Yoga", HireDate = new DateOnly(2023, 6, 10), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 4, FirstName = "David", LastName = "Thompson", Email = "david.thompson@zenith.com", Phone = "555-1004", Bio = "Spin and cardio specialist passionate about endurance training", Specializations = "Spin,HIIT", HireDate = new DateOnly(2023, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Class Types
        modelBuilder.Entity<ClassType>().HasData(
            new ClassType { Id = 1, Name = "Yoga", Description = "Mindful movement and breathing exercises for flexibility and relaxation", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 2, Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 3, Name = "Spin", Description = "Indoor cycling class with energizing music and varied intensity", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 4, Name = "Pilates", Description = "Core-strengthening exercises emphasizing posture and alignment", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 5, Name = "Boxing", Description = "Boxing-inspired cardio and strength training", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 6, Name = "Meditation", Description = "Guided meditation and mindfulness practice", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Class Schedules - relative to a base date. We'll use fixed dates for seed data.
        var baseDate = new DateTime(2025, 7, 21, 0, 0, 0, DateTimeKind.Utc); // Monday
        modelBuilder.Entity<ClassSchedule>().HasData(
            new ClassSchedule { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddHours(7), EndTime = baseDate.AddHours(8), Capacity = 20, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 3, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddHours(17), EndTime = baseDate.AddHours(17).AddMinutes(45), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 4, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(1).AddHours(8), EndTime = baseDate.AddDays(1).AddHours(9), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 5, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(1).AddHours(18), EndTime = baseDate.AddDays(1).AddHours(19), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 6, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(2).AddHours(7), EndTime = baseDate.AddDays(2).AddHours(7).AddMinutes(30), Capacity = 25, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 7, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(2).AddHours(10), EndTime = baseDate.AddDays(2).AddHours(11), Capacity = 20, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 8, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(3).AddHours(6), EndTime = baseDate.AddDays(3).AddHours(6).AddMinutes(45), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 9, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddDays(3).AddHours(17), EndTime = baseDate.AddDays(3).AddHours(17).AddMinutes(45), Capacity = 12, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 10, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(4).AddHours(18), EndTime = baseDate.AddDays(4).AddHours(19), Capacity = 2, CurrentEnrollment = 2, WaitlistCount = 1, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 11, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(5).AddHours(9), EndTime = baseDate.AddDays(5).AddHours(10), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 12, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(6).AddHours(8), EndTime = baseDate.AddDays(6).AddHours(8).AddMinutes(30), Capacity = 25, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Zen Room", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 13, ClassTypeId = 1, InstructorId = 3, StartTime = baseDate.AddDays(4).AddHours(7), EndTime = baseDate.AddDays(4).AddHours(8), Capacity = 20, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now }
        );

        // Memberships
        modelBuilder.Entity<Membership>().HasData(
            new Membership { Id = 1, MemberId = 1, MembershipPlanId = 2, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 2, MemberId = 2, MembershipPlanId = 3, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 8, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 7, MemberId = 7, MembershipPlanId = 2, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 8, MemberId = 8, MembershipPlanId = 1, StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2025, 6, 1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now }
        );

        // Bookings
        modelBuilder.Entity<Booking>().HasData(
            new Booking { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 3, ClassScheduleId = 1, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 4, ClassScheduleId = 2, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 5, ClassScheduleId = 2, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 6, ClassScheduleId = 3, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 7, ClassScheduleId = 3, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 8, ClassScheduleId = 4, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 9, ClassScheduleId = 5, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 10, ClassScheduleId = 5, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 11, ClassScheduleId = 6, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 12, ClassScheduleId = 7, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 13, ClassScheduleId = 8, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 14, ClassScheduleId = 9, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 15, ClassScheduleId = 10, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 16, ClassScheduleId = 10, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 17, ClassScheduleId = 10, MemberId = 4, BookingDate = now, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 18, ClassScheduleId = 11, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now }
        );
    }
}
