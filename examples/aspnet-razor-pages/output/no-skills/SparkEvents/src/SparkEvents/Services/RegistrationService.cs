using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class RegistrationService : IRegistrationService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(SparkEventsDbContext db, ILogger<RegistrationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(Registration? Registration, string? Error)> RegisterAsync(
        int eventId, int attendeeId, int ticketTypeId, string? specialRequests)
    {
        var evt = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt == null)
            return (null, "Event not found.");

        // Check registration window
        var now = DateTime.UtcNow;
        if (now < evt.RegistrationOpenDate)
            return (null, "Registration is not yet open for this event.");
        if (now > evt.RegistrationCloseDate)
            return (null, "Registration has closed for this event.");

        // Check event status
        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut)
            return (null, "This event is not accepting registrations.");

        // Check duplicate registration
        var existingReg = await _db.Registrations
            .AnyAsync(r => r.EventId == eventId && r.AttendeeId == attendeeId && r.Status != RegistrationStatus.Cancelled);
        if (existingReg)
            return (null, "This attendee is already registered for this event.");

        var ticketType = evt.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticketType == null || !ticketType.IsActive)
            return (null, "Invalid or inactive ticket type.");

        // Determine price
        decimal price;
        if (evt.EarlyBirdDeadline.HasValue && ticketType.EarlyBirdPrice.HasValue && now < evt.EarlyBirdDeadline.Value)
            price = ticketType.EarlyBirdPrice.Value;
        else
            price = ticketType.Price;

        // Generate confirmation number
        var confirmationNumber = await GenerateConfirmationNumberAsync(evt);

        // Determine status
        RegistrationStatus status;
        int? waitlistPosition = null;

        bool ticketTypeSoldOut = ticketType.QuantitySold >= ticketType.Quantity;
        bool eventAtCapacity = evt.CurrentRegistrations >= evt.TotalCapacity;

        if (ticketTypeSoldOut)
            return (null, $"The '{ticketType.Name}' ticket type is sold out. Please choose a different ticket type.");

        if (eventAtCapacity)
        {
            status = RegistrationStatus.Waitlisted;
            waitlistPosition = evt.WaitlistCount + 1;
            evt.WaitlistCount++;
        }
        else
        {
            status = RegistrationStatus.Confirmed;
            evt.CurrentRegistrations++;
            ticketType.QuantitySold++;

            // Check if event is now sold out
            if (evt.CurrentRegistrations >= evt.TotalCapacity)
                evt.Status = EventStatus.SoldOut;
        }

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            TicketTypeId = ticketTypeId,
            ConfirmationNumber = confirmationNumber,
            Status = status,
            AmountPaid = price,
            WaitlistPosition = waitlistPosition,
            RegistrationDate = now,
            SpecialRequests = specialRequests,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Registrations.Add(registration);
        evt.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registration {ConfirmationNumber} created for event {EventId}, attendee {AttendeeId}, status: {Status}",
            confirmationNumber, eventId, attendeeId, status);

        return (registration, null);
    }

    public async Task<Registration?> GetByIdAsync(int id)
    {
        return await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber)
    {
        return await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == confirmationNumber);
    }

    public async Task<(List<Registration> Items, int TotalCount)> GetEventRosterAsync(
        int eventId, string? search, int page, int pageSize)
    {
        var query = _db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.Status != RegistrationStatus.Waitlisted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r =>
                r.Attendee.FirstName.ToLower().Contains(s) ||
                r.Attendee.LastName.ToLower().Contains(s) ||
                r.ConfirmationNumber.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Attendee.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId)
    {
        return await _db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync();
    }

    public async Task<string?> CancelRegistrationAsync(int id, string? reason)
    {
        var reg = await _db.Registrations
            .Include(r => r.Event)
                .ThenInclude(e => e.Registrations)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reg == null) return "Registration not found.";
        if (reg.Status == RegistrationStatus.Cancelled) return "Registration is already cancelled.";
        if (reg.Status == RegistrationStatus.CheckedIn) return "Cannot cancel a checked-in registration.";

        // Check 24-hour policy
        if (reg.Event.StartDate <= DateTime.UtcNow.AddHours(24))
            return "Cancellations are not allowed within 24 hours of the event start.";

        var wasConfirmed = reg.Status == RegistrationStatus.Confirmed;
        reg.Status = RegistrationStatus.Cancelled;
        reg.CancellationDate = DateTime.UtcNow;
        reg.CancellationReason = reason;
        reg.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            reg.Event.CurrentRegistrations--;
            reg.TicketType.QuantitySold--;

            // Promote first waitlisted registration
            var firstWaitlisted = await _db.Registrations
                .Include(r => r.TicketType)
                .Where(r => r.EventId == reg.EventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (firstWaitlisted != null)
            {
                firstWaitlisted.Status = RegistrationStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = DateTime.UtcNow;
                reg.Event.CurrentRegistrations++;
                reg.Event.WaitlistCount--;
                firstWaitlisted.TicketType.QuantitySold++;
                _logger.LogInformation("Waitlist promotion: Registration {Id} promoted for event {EventId}",
                    firstWaitlisted.Id, reg.EventId);

                // Re-number remaining waitlist
                var remaining = await _db.Registrations
                    .Where(r => r.EventId == reg.EventId && r.Status == RegistrationStatus.Waitlisted)
                    .OrderBy(r => r.WaitlistPosition)
                    .ToListAsync();
                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].WaitlistPosition = i + 1;
            }

            // Update event status
            if (reg.Event.Status == EventStatus.SoldOut && reg.Event.CurrentRegistrations < reg.Event.TotalCapacity)
                reg.Event.Status = EventStatus.Published;
        }
        else if (reg.Status == RegistrationStatus.Waitlisted)
        {
            reg.Event.WaitlistCount--;
        }

        reg.Event.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registration {Id} cancelled for event {EventId}", id, reg.EventId);
        return null;
    }

    public async Task<List<Registration>> GetRecentRegistrationsAsync(int count)
    {
        return await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .OrderByDescending(r => r.RegistrationDate)
            .Take(count)
            .ToListAsync();
    }

    private async Task<string> GenerateConfirmationNumberAsync(Event evt)
    {
        var dateStr = evt.StartDate.ToString("yyyyMMdd");
        var prefix = $"SPK-{dateStr}-";

        var lastReg = await _db.Registrations
            .Where(r => r.EventId == evt.Id)
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastReg != null && lastReg.ConfirmationNumber.StartsWith(prefix))
        {
            var numPart = lastReg.ConfirmationNumber[(prefix.Length)..];
            if (int.TryParse(numPart, out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
