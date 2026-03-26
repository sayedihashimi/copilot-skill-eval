using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class InspectionService : IInspectionService
{
    private readonly AppDbContext _context;

    public InspectionService(AppDbContext context) => _context = context;

    public async Task<PaginatedList<Inspection>> GetInspectionsAsync(InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize)
    {
        var query = _context.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease)
            .AsQueryable();

        if (type.HasValue) query = query.Where(i => i.InspectionType == type.Value);
        if (unitId.HasValue) query = query.Where(i => i.UnitId == unitId.Value);
        if (fromDate.HasValue) query = query.Where(i => i.ScheduledDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.ScheduledDate <= toDate.Value);

        query = query.OrderByDescending(i => i.ScheduledDate);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Inspection>(items, count, pageNumber, pageSize);
    }

    public async Task<Inspection?> GetByIdAsync(int id) =>
        await _context.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Inspection?> GetWithDetailsAsync(int id) => await GetByIdAsync(id);

    public async Task CreateAsync(Inspection inspection)
    {
        inspection.CreatedAt = DateTime.UtcNow;
        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int id, OverallCondition condition, string? notes, bool followUpRequired)
    {
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null) return (false, "Inspection not found.");

        if (inspection.CompletedDate.HasValue)
            return (false, "Inspection has already been completed.");

        inspection.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
