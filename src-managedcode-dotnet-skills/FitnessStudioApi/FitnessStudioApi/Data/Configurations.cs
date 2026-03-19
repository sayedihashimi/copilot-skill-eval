using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessStudioApi.Data.Configurations;

public sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)");
    }
}

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.LastName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Email).IsRequired();
        builder.HasIndex(m => m.Email).IsUnique();
        builder.Property(m => m.EmergencyContactName).HasMaxLength(200).IsRequired();
    }
}

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.PaymentStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(m => m.Member)
            .WithMany(m => m.Memberships)
            .HasForeignKey(m => m.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MembershipPlan)
            .WithMany(p => p.Memberships)
            .HasForeignKey(m => m.MembershipPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.MemberId, m.Status });
    }
}

public sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(i => i.LastName).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Email).IsRequired();
        builder.HasIndex(i => i.Email).IsUnique();
        builder.Property(i => i.Bio).HasMaxLength(1000);
    }
}

public sealed class ClassTypeConfiguration : IEntityTypeConfiguration<ClassType>
{
    public void Configure(EntityTypeBuilder<ClassType> builder)
    {
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(ct => ct.Name).IsUnique();
        builder.Property(ct => ct.Description).HasMaxLength(500);
        builder.Property(ct => ct.DifficultyLevel).HasConversion<string>().HasMaxLength(20);
    }
}

public sealed class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Room).HasMaxLength(50).IsRequired();
        builder.Property(cs => cs.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(cs => cs.ClassType)
            .WithMany(ct => ct.ClassSchedules)
            .HasForeignKey(cs => cs.ClassTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Instructor)
            .WithMany(i => i.ClassSchedules)
            .HasForeignKey(cs => cs.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cs => cs.StartTime);
        builder.HasIndex(cs => new { cs.InstructorId, cs.StartTime });
    }
}

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(b => b.ClassSchedule)
            .WithMany(cs => cs.Bookings)
            .HasForeignKey(b => b.ClassScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Member)
            .WithMany(m => m.Bookings)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.ClassScheduleId, b.MemberId });
        builder.HasIndex(b => new { b.MemberId, b.Status });
    }
}
