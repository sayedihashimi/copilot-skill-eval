using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
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

        // MembershipPlan
        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        // Member
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Membership
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MembershipPlan)
                .WithMany()
                .HasForeignKey(e => e.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.Property(e => e.PaymentStatus)
                .HasConversion<string>();
        });

        // Instructor
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ClassType
        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.DifficultyLevel)
                .HasConversion<string>();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasOne(e => e.ClassType)
                .WithMany()
                .HasForeignKey(e => e.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasConversion<string>();
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(e => e.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(e => e.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasConversion<string>();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetProperty("UpdatedAt") is { } prop)
            {
                prop.SetValue(entry.Entity, DateTime.UtcNow);
            }
        }
    }
}
