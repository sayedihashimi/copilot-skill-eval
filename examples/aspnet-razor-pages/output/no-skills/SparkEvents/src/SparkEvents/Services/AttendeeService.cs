using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class AttendeeService : IAttendeeService
{
    private readonly SparkEventsDbContext _db;

    public AttendeeService(SparkEventsDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Attendee> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _db.Attendees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(s) ||
                a.LastName.ToLower().Contains(s) ||
                a.Email.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Attendee>> GetAllAsync()
    {
        return await _db.Attendees.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToListAsync();
    }

    public async Task<Attendee?> GetByIdAsync(int id)
    {
        return await _db.Attendees.FindAsync(id);
    }

    public async Task<Attendee?> GetByIdWithRegistrationsAsync(int id)
    {
        return await _db.Attendees
            .Include(a => a.Registrations)
                .ThenInclude(r => r.Event)
            .Include(a => a.Registrations)
                .ThenInclude(r => r.TicketType)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Attendee?> GetByEmailAsync(string email)
    {
        return await _db.Attendees.FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
    }

    public async Task CreateAsync(Attendee attendee)
    {
        attendee.CreatedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;
        _db.Attendees.Add(attendee);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Attendee attendee)
    {
        attendee.UpdatedAt = DateTime.UtcNow;
        _db.Attendees.Update(attendee);
        await _db.SaveChangesAsync();
    }
}
