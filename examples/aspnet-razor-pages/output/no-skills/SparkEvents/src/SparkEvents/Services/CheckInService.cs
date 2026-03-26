using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CheckInService : ICheckInService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(SparkEventsDbContext db, ILogger<CheckInService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(CheckIn? CheckIn, string? Error)> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes)
    {
        var reg = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == registrationId);

        if (reg == null) return (null, "Registration not found.");
        if (reg.Status != RegistrationStatus.Confirmed) return (null, "Only confirmed registrations can be checked in.");
        if (reg.CheckIn != null) return (null, "This registration has already been checked in.");

        // Check-in window: StartDate - 1 hour to EndDate
        var now = DateTime.UtcNow;
        var windowStart = reg.Event.StartDate.AddHours(-1);
        var windowEnd = reg.Event.EndDate;

        if (now < windowStart || now > windowEnd)
            return (null, $"Check-in is only available between {windowStart:g} and {windowEnd:g}.");

        var checkIn = new CheckIn
        {
            RegistrationId = registrationId,
            CheckInTime = now,
            CheckedInBy = checkedInBy,
            Notes = notes
        };

        reg.Status = RegistrationStatus.CheckedIn;
        reg.CheckInTime = now;
        reg.UpdatedAt = now;

        _db.CheckIns.Add(checkIn);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Check-in processed for registration {RegistrationId} by {CheckedInBy}", registrationId, checkedInBy);
        return (checkIn, null);
    }

    public async Task<(List<Registration> Items, int TotalCheckedIn, int TotalConfirmed)> GetCheckInDashboardAsync(int eventId, string? search)
    {
        var query = _db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId &&
                        (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));

        var allConfirmed = await query.CountAsync();
        var checkedIn = await query.CountAsync(r => r.Status == RegistrationStatus.CheckedIn);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r =>
                r.Attendee.FirstName.ToLower().Contains(s) ||
                r.Attendee.LastName.ToLower().Contains(s) ||
                r.ConfirmationNumber.ToLower().Contains(s));
        }

        var items = await query
            .OrderBy(r => r.Attendee.LastName)
            .ToListAsync();

        return (items, checkedIn, allConfirmed);
    }

    public async Task<Registration?> LookupForCheckInAsync(int eventId, string searchTerm)
    {
        var s = searchTerm.ToLower();
        return await _db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed)
            .FirstOrDefaultAsync(r =>
                r.ConfirmationNumber.ToLower() == s ||
                (r.Attendee.FirstName.ToLower() + " " + r.Attendee.LastName.ToLower()).Contains(s));
    }
}
