using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class VenueService : IVenueService
{
    private readonly SparkEventsDbContext _db;

    public VenueService(SparkEventsDbContext db)
    {
        _db = db;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        return await _db.Venues.OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _db.Venues.FindAsync(id);
    }

    public async Task<Venue?> GetByIdWithEventsAsync(int id)
    {
        return await _db.Venues
            .Include(v => v.Events.Where(e => e.StartDate >= DateTime.UtcNow).OrderBy(e => e.StartDate))
                .ThenInclude(e => e.EventCategory)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task CreateAsync(Venue venue)
    {
        venue.CreatedAt = DateTime.UtcNow;
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Venue venue)
    {
        _db.Venues.Update(venue);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await HasFutureEventsAsync(id))
            return false;

        var venue = await _db.Venues.FindAsync(id);
        if (venue == null) return false;

        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasFutureEventsAsync(int id)
    {
        return await _db.Events.AnyAsync(e => e.VenueId == id && e.StartDate >= DateTime.UtcNow);
    }
}
