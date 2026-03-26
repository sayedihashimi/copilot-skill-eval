using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class EventService : IEventService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<EventService> _logger;

    public EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(List<Event> Items, int TotalCount)> GetFilteredAsync(EventFilterModel filter)
    {
        var query = _db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(s));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(e => e.EventCategoryId == filter.CategoryId.Value);

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(e => e.StartDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(e => e.StartDate <= filter.EndDate.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.IsFeatured)
            .ThenBy(e => e.StartDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByIdWithDetailsAsync(int id)
    {
        return await _db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendee)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateAsync(Event evt)
    {
        evt.CreatedAt = DateTime.UtcNow;
        evt.UpdatedAt = DateTime.UtcNow;
        _db.Events.Add(evt);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event evt)
    {
        evt.UpdatedAt = DateTime.UtcNow;
        _db.Events.Update(evt);
        await _db.SaveChangesAsync();
    }

    public async Task<string?> PublishAsync(int id)
    {
        var evt = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null) return "Event not found.";
        if (evt.Status != EventStatus.Draft) return "Only draft events can be published.";
        if (!evt.TicketTypes.Any(t => t.IsActive)) return "At least one active ticket type is required to publish.";

        evt.Status = EventStatus.Published;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Event {EventId} '{Title}' published.", evt.Id, evt.Title);
        return null;
    }

    public async Task<string?> CancelEventAsync(int id, string reason)
    {
        var evt = await _db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evt == null) return "Event not found.";
        if (evt.Status == EventStatus.Completed || evt.Status == EventStatus.Cancelled)
            return "Cannot cancel a completed or already-cancelled event.";

        evt.Status = EventStatus.Cancelled;
        evt.CancellationReason = reason;
        evt.UpdatedAt = DateTime.UtcNow;

        // Cancel all non-cancelled registrations
        foreach (var reg in evt.Registrations.Where(r => r.Status != RegistrationStatus.Cancelled))
        {
            reg.Status = RegistrationStatus.Cancelled;
            reg.CancellationDate = DateTime.UtcNow;
            reg.CancellationReason = "Event cancelled by organizer";
            reg.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Event {EventId} '{Title}' cancelled. Reason: {Reason}", evt.Id, evt.Title, reason);
        return null;
    }

    public async Task<string?> CompleteEventAsync(int id)
    {
        var evt = await _db.Events.FindAsync(id);
        if (evt == null) return "Event not found.";
        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut)
            return "Only published or sold out events can be completed.";
        if (evt.EndDate > DateTime.UtcNow)
            return "Cannot complete an event before its end date.";

        evt.Status = EventStatus.Completed;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Event {EventId} '{Title}' marked as completed.", evt.Id, evt.Title);
        return null;
    }

    public async Task<List<TicketType>> GetTicketTypesAsync(int eventId)
    {
        return await _db.TicketTypes
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<TicketType?> GetTicketTypeByIdAsync(int id)
    {
        return await _db.TicketTypes.FindAsync(id);
    }

    public async Task CreateTicketTypeAsync(TicketType ticketType)
    {
        ticketType.CreatedAt = DateTime.UtcNow;
        _db.TicketTypes.Add(ticketType);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateTicketTypeAsync(TicketType ticketType)
    {
        _db.TicketTypes.Update(ticketType);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(int days)
    {
        var now = DateTime.UtcNow;
        var end = now.AddDays(days);
        return await _db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= now && e.StartDate <= end &&
                        (e.Status == EventStatus.Published || e.Status == EventStatus.SoldOut))
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<List<Event>> GetTodaysEventsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await _db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
            .Where(e => e.StartDate >= today && e.StartDate < tomorrow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetTotalEventsCountAsync()
    {
        return await _db.Events.CountAsync();
    }

    public async Task<int> GetEventsThisMonthCountAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        return await _db.Events.CountAsync(e => e.StartDate >= startOfMonth && e.StartDate < endOfMonth);
    }

    public async Task<int> GetTotalRegistrationsCountAsync()
    {
        return await _db.Registrations.CountAsync(r => r.Status != RegistrationStatus.Cancelled);
    }
}
