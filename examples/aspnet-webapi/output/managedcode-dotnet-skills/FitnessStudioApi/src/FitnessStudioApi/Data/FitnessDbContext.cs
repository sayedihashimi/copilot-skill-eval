using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
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
        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EmergencyContactPhone).IsRequired();
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Memberships)
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MembershipPlan)
                  .WithMany()
                  .HasForeignKey(e => e.MembershipPlanId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.Property(e => e.PaymentStatus)
                  .HasConversion<string>();
            entity.HasIndex(e => new { e.MemberId, e.Status });
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.Bio).HasMaxLength(1000);
        });

        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DifficultyLevel)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ClassType)
                  .WithMany()
                  .HasForeignKey(e => e.ClassTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Instructor)
                  .WithMany(i => i.ClassSchedules)
                  .HasForeignKey(e => e.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Room).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.HasIndex(e => new { e.StartTime, e.EndTime });
            entity.HasIndex(e => e.InstructorId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ClassSchedule)
                  .WithMany(cs => cs.Bookings)
                  .HasForeignKey(e => e.ClassScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Member)
                  .WithMany(m => m.Bookings)
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Status)
                  .HasConversion<string>();
            entity.HasIndex(e => new { e.ClassScheduleId, e.MemberId });
            entity.HasIndex(e => new { e.MemberId, e.Status });
        });
    }
}
