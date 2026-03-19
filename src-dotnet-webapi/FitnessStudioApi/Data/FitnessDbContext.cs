using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class FitnessDbContext : DbContext
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

        // MembershipPlan
        modelBuilder.Entity<MembershipPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.Phone).IsRequired();
            e.Property(x => x.EmergencyContactName).HasMaxLength(200).IsRequired();
            e.Property(x => x.EmergencyContactPhone).IsRequired();
        });

        // Membership
        modelBuilder.Entity<Membership>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.PaymentStatus).HasConversion<string>();
            e.HasOne(x => x.Member).WithMany(m => m.Memberships).HasForeignKey(x => x.MemberId);
            e.HasOne(x => x.MembershipPlan).WithMany(p => p.Memberships).HasForeignKey(x => x.MembershipPlanId);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.Phone).IsRequired();
            e.Property(x => x.Bio).HasMaxLength(1000);
        });

        // ClassType
        modelBuilder.Entity<ClassType>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.DifficultyLevel).HasConversion<string>();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Room).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.ClassType).WithMany(ct => ct.ClassSchedules).HasForeignKey(x => x.ClassTypeId);
            e.HasOne(x => x.Instructor).WithMany(i => i.ClassSchedules).HasForeignKey(x => x.InstructorId);
        });

        // Booking
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.ClassSchedule).WithMany(cs => cs.Bookings).HasForeignKey(x => x.ClassScheduleId);
            e.HasOne(x => x.Member).WithMany(m => m.Bookings).HasForeignKey(x => x.MemberId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Membership Plans
        modelBuilder.Entity<MembershipPlan>().HasData(
            new MembershipPlan { Id = 1, Name = "Basic", Description = "Access to standard classes, 3 bookings per week", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 2, Name = "Premium", Description = "Access to all classes including premium, 5 bookings per week", DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new MembershipPlan { Id = 3, Name = "Elite", Description = "Unlimited access to all classes", DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Members
        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = new DateOnly(2024, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 2, FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0201", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Lin Chen", EmergencyContactPhone = "555-0202", JoinDate = new DateOnly(2024, 7, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 3, FirstName = "Sophia", LastName = "Williams", Email = "sophia.williams@email.com", Phone = "555-0301", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "James Williams", EmergencyContactPhone = "555-0302", JoinDate = new DateOnly(2024, 8, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 4, FirstName = "Raj", LastName = "Patel", Email = "raj.patel@email.com", Phone = "555-0401", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0402", JoinDate = new DateOnly(2024, 9, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 5, FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0501", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Tom Davis", EmergencyContactPhone = "555-0502", JoinDate = new DateOnly(2024, 10, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 6, FirstName = "Liam", LastName = "O'Brien", Email = "liam.obrien@email.com", Phone = "555-0601", DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Fiona O'Brien", EmergencyContactPhone = "555-0602", JoinDate = new DateOnly(2024, 4, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 7, FirstName = "Yuki", LastName = "Tanaka", Email = "yuki.tanaka@email.com", Phone = "555-0701", DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "Kenji Tanaka", EmergencyContactPhone = "555-0702", JoinDate = new DateOnly(2024, 5, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Member { Id = 8, FirstName = "Olivia", LastName = "Martinez", Email = "olivia.martinez@email.com", Phone = "555-0801", DateOfBirth = new DateOnly(1987, 4, 18), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0802", JoinDate = new DateOnly(2024, 3, 1), IsActive = false, CreatedAt = now, UpdatedAt = now }
        );

        // Instructors
        modelBuilder.Entity<Instructor>().HasData(
            new Instructor { Id = 1, FirstName = "Sarah", LastName = "Miller", Email = "sarah.miller@zenithfitness.com", Phone = "555-1001", Bio = "Certified yoga and pilates instructor with 10 years of experience", Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2022, 1, 15), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 2, FirstName = "Jake", LastName = "Thompson", Email = "jake.thompson@zenithfitness.com", Phone = "555-1002", Bio = "Former professional athlete specializing in high-intensity training", Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2022, 3, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 3, FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@zenithfitness.com", Phone = "555-1003", Bio = "Dance and cardio fitness specialist", Specializations = "Spin, HIIT, Yoga", HireDate = new DateOnly(2023, 6, 1), IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Instructor { Id = 4, FirstName = "David", LastName = "Kim", Email = "david.kim@zenithfitness.com", Phone = "555-1004", Bio = "Martial arts and boxing coach with competition experience", Specializations = "Boxing, HIIT", HireDate = new DateOnly(2023, 9, 15), IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Class Types (Boxing and Spin are premium)
        modelBuilder.Entity<ClassType>().HasData(
            new ClassType { Id = 1, Name = "Yoga", Description = "Traditional yoga focusing on flexibility and mindfulness", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 2, Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 3, Name = "Spin", Description = "Indoor cycling with energizing music and coaching", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 4, Name = "Pilates", Description = "Core-strengthening exercises for all fitness levels", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 5, Name = "Boxing", Description = "Premium boxing class with personal attention", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new ClassType { Id = 6, Name = "Meditation", Description = "Guided meditation for stress relief and mental clarity", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        // Memberships - using fixed dates relative to seed
        var today = new DateOnly(2025, 7, 1);
        modelBuilder.Entity<Membership>().HasData(
            // Active memberships
            new Membership { Id = 1, MemberId = 1, MembershipPlanId = 3, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 2, MemberId = 2, MembershipPlanId = 2, StartDate = new DateOnly(2025, 6, 15), EndDate = new DateOnly(2025, 7, 15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 3, MemberId = 3, MembershipPlanId = 1, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 4, MemberId = 4, MembershipPlanId = 2, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 5, MemberId = 5, MembershipPlanId = 1, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { Id = 6, MemberId = 6, MembershipPlanId = 3, StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 7, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7
            new Membership { Id = 7, MemberId = 7, MembershipPlanId = 1, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2025, 4, 1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Cancelled membership for member 8
            new Membership { Id = 8, MemberId = 8, MembershipPlanId = 2, StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2025, 4, 1), Status = MembershipStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now, UpdatedAt = now }
        );

        // Class Schedules - using dates relative to a fixed future date
        // We'll use dates in late July 2025 to ensure they're in the future
        var baseDate = new DateTime(2025, 7, 21, 0, 0, 0, DateTimeKind.Utc); // Monday
        modelBuilder.Entity<ClassSchedule>().HasData(
            // Monday
            new ClassSchedule { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddHours(8), EndTime = baseDate.AddHours(9), Capacity = 20, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddHours(10), EndTime = baseDate.AddHours(10).AddMinutes(45), Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Tuesday
            new ClassSchedule { Id = 3, ClassTypeId = 3, InstructorId = 3, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(9).AddMinutes(45), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 4, ClassTypeId = 4, InstructorId = 1, StartTime = baseDate.AddDays(1).AddHours(14), EndTime = baseDate.AddDays(1).AddHours(15), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Wednesday
            new ClassSchedule { Id = 5, ClassTypeId = 5, InstructorId = 4, StartTime = baseDate.AddDays(2).AddHours(11), EndTime = baseDate.AddDays(2).AddHours(12), Capacity = 10, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 6, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(17).AddMinutes(30), Capacity = 25, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Thursday
            new ClassSchedule { Id = 7, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(3).AddHours(8), EndTime = baseDate.AddDays(3).AddHours(9), Capacity = 20, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 8, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(3).AddHours(18), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(45), Capacity = 15, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Friday - one class at full capacity with waitlist
            new ClassSchedule { Id = 9, ClassTypeId = 3, InstructorId = 3, StartTime = baseDate.AddDays(4).AddHours(9), EndTime = baseDate.AddDays(4).AddHours(9).AddMinutes(45), Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { Id = 10, ClassTypeId = 5, InstructorId = 4, StartTime = baseDate.AddDays(4).AddHours(16), EndTime = baseDate.AddDays(4).AddHours(17), Capacity = 10, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Saturday
            new ClassSchedule { Id = 11, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(5).AddHours(10), EndTime = baseDate.AddDays(5).AddHours(11), Capacity = 20, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new ClassSchedule { Id = 12, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(5).AddHours(14), EndTime = baseDate.AddDays(5).AddHours(14).AddMinutes(45), Capacity = 15, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Main Floor", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now }
        );

        // Bookings
        modelBuilder.Entity<Booking>().HasData(
            // Class 1 (Yoga Mon) - 3 confirmed
            new Booking { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 3, ClassScheduleId = 1, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 2 (HIIT Mon) - 2 confirmed
            new Booking { Id = 4, ClassScheduleId = 2, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 5, ClassScheduleId = 2, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 3 (Spin Tue, premium) - 2 confirmed
            new Booking { Id = 6, ClassScheduleId = 3, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 7, ClassScheduleId = 3, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 4 (Pilates Tue) - 1 confirmed
            new Booking { Id = 8, ClassScheduleId = 4, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 5 (Boxing Wed, premium) - 2 confirmed
            new Booking { Id = 9, ClassScheduleId = 5, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 10, ClassScheduleId = 5, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 6 (Meditation Wed) - 1 attended
            new Booking { Id = 11, ClassScheduleId = 6, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 7 (Yoga Thu) - 2 confirmed
            new Booking { Id = 12, ClassScheduleId = 7, MemberId = 3, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 13, ClassScheduleId = 7, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 8 (HIIT Thu) - 1 confirmed
            new Booking { Id = 14, ClassScheduleId = 8, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 9 (Spin Fri, FULL with waitlist) - 3 confirmed + 2 waitlisted
            new Booking { Id = 15, ClassScheduleId = 9, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 16, ClassScheduleId = 9, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 17, ClassScheduleId = 9, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 18, ClassScheduleId = 9, MemberId = 4, BookingDate = now, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new Booking { Id = 19, ClassScheduleId = 9, MemberId = 1, BookingDate = now, Status = BookingStatus.Cancelled, CancellationDate = now, CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },
            // Class 10 (Boxing Fri) - 1 confirmed
            new Booking { Id = 20, ClassScheduleId = 10, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Class 11 (Yoga Sat) - 1 no-show
            new Booking { Id = 21, ClassScheduleId = 11, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now }
        );
    }
}
