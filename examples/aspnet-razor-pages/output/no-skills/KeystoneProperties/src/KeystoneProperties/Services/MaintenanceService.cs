using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(AppDbContext context, ILogger<MaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize)
    {
        var query = _context.MaintenanceRequests
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .AsQueryable();

        if (status.HasValue) query = query.Where(m => m.Status == status.Value);
        if (priority.HasValue) query = query.Where(m => m.Priority == priority.Value);
        if (propertyId.HasValue) query = query.Where(m => m.Unit.PropertyId == propertyId.Value);
        if (category.HasValue) query = query.Where(m => m.Category == category.Value);

        query = query.OrderByDescending(m => m.SubmittedDate);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<MaintenanceRequest>(items, count, pageNumber, pageSize);
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(int id) =>
        await _context.MaintenanceRequests
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<MaintenanceRequest?> GetWithDetailsAsync(int id) => await GetByIdAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(MaintenanceRequest request)
    {
        if (request.Priority == MaintenancePriority.Emergency && string.IsNullOrWhiteSpace(request.AssignedTo))
            return (false, "Emergency maintenance requests must have an assigned worker.");

        request.SubmittedDate = DateTime.UtcNow;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        if (request.Priority == MaintenancePriority.Emergency)
        {
            request.Status = MaintenanceStatus.Assigned;
            request.AssignedDate = DateTime.UtcNow;

            // Set unit to Maintenance for emergency
            var unit = await _context.Units.FindAsync(request.UnitId);
            if (unit != null) unit.Status = UnitStatus.Maintenance;
        }

        _context.MaintenanceRequests.Add(request);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Maintenance request created: {Title} (ID: {Id})", request.Title, request.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost)
    {
        var request = await _context.MaintenanceRequests.Include(m => m.Unit).FirstOrDefaultAsync(m => m.Id == id);
        if (request == null) return (false, "Maintenance request not found.");

        // Validate status transitions
        var validTransitions = request.Status switch
        {
            MaintenanceStatus.Submitted => new[] { MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled },
            MaintenanceStatus.Assigned => new[] { MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled },
            MaintenanceStatus.InProgress => new[] { MaintenanceStatus.Completed, MaintenanceStatus.Cancelled },
            _ => Array.Empty<MaintenanceStatus>()
        };

        if (!validTransitions.Contains(newStatus))
            return (false, $"Cannot transition from {request.Status} to {newStatus}.");

        if (newStatus == MaintenanceStatus.Assigned)
        {
            if (string.IsNullOrWhiteSpace(assignedTo))
                return (false, "Assigned To is required when assigning a maintenance request.");
            request.AssignedTo = assignedTo;
            request.AssignedDate = DateTime.UtcNow;
        }

        if (newStatus == MaintenanceStatus.Completed)
        {
            request.CompletedDate = DateTime.UtcNow;
            request.CompletionNotes = completionNotes;

            // Restore unit status if emergency
            if (request.Priority == MaintenancePriority.Emergency)
            {
                var hasActiveLease = await _context.Leases.AnyAsync(l => l.UnitId == request.UnitId && l.Status == LeaseStatus.Active);
                request.Unit.Status = hasActiveLease ? UnitStatus.Occupied : UnitStatus.Available;
            }
        }

        if (estimatedCost.HasValue) request.EstimatedCost = estimatedCost;
        if (actualCost.HasValue) request.ActualCost = actualCost;

        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Maintenance request {Id} status changed to {Status}", id, newStatus);
        return (true, null);
    }

    public async Task<int> GetOpenRequestCountAsync() =>
        await _context.MaintenanceRequests.CountAsync(m =>
            m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled);
}
