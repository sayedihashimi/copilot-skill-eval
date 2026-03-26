using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CategoryService : ICategoryService
{
    private readonly SparkEventsDbContext _db;

    public CategoryService(SparkEventsDbContext db)
    {
        _db = db;
    }

    public async Task<List<EventCategory>> GetAllAsync()
    {
        return await _db.EventCategories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<EventCategory?> GetByIdAsync(int id)
    {
        return await _db.EventCategories.FindAsync(id);
    }

    public async Task CreateAsync(EventCategory category)
    {
        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventCategory category)
    {
        _db.EventCategories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await HasEventsAsync(id))
            return false;

        var category = await _db.EventCategories.FindAsync(id);
        if (category == null) return false;

        _db.EventCategories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasEventsAsync(int id)
    {
        return await _db.Events.AnyAsync(e => e.EventCategoryId == id);
    }
}
