using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public class SparkEventsDbContext : DbContext
{
    public SparkEventsDbContext(DbContextOptions<SparkEventsDbContext> options) : base(options) { }

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Attendee>(e =>
        {
            e.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Registration>(e =>
        {
            e.HasIndex(r => r.ConfirmationNumber).IsUnique();
        });

        modelBuilder.Entity<CheckIn>(e =>
        {
            e.HasIndex(c => c.RegistrationId).IsUnique();
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.HasOne(ev => ev.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(ev => ev.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(ev => ev.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(ev => ev.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketType>(e =>
        {
            e.HasOne(t => t.Event)
                .WithMany(ev => ev.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(e =>
        {
            e.HasOne(r => r.Event)
                .WithMany(ev => ev.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Attendee)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.TicketType)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckIn>(e =>
        {
            e.HasOne(c => c.Registration)
                .WithOne(r => r.CheckIn)
                .HasForeignKey<CheckIn>(c => c.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
