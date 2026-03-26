using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
{
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
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        // Member
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
        });

        // Membership
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasConversion<string>();

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Memberships)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MembershipPlan)
                .WithMany(p => p.Memberships)
                .HasForeignKey(e => e.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Instructor
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(1000);
        });

        // ClassType
        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DifficultyLevel).HasConversion<string>();
        });

        // ClassSchedule
        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CancellationReason).HasMaxLength(500);

            entity.HasOne(e => e.ClassType)
                .WithMany(ct => ct.ClassSchedules)
                .HasForeignKey(e => e.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.ClassSchedules)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CancellationReason).HasMaxLength(500);

            entity.HasOne(e => e.ClassSchedule)
                .WithMany(cs => cs.Bookings)
                .HasForeignKey(e => e.ClassScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;

            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") is not null)
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
        }
    }
}
