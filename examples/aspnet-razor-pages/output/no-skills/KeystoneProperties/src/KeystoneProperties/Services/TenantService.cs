using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TenantService> _logger;

    public TenantService(AppDbContext context, ILogger<TenantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Tenant>> GetTenantsAsync(string? search, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.FirstName.Contains(search) || t.LastName.Contains(search) || t.Email.Contains(search));
        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Tenant>(items, count, pageNumber, pageSize);
    }

    public async Task<Tenant?> GetByIdAsync(int id) =>
        await _context.Tenants.FindAsync(id);

    public async Task<Tenant?> GetWithDetailsAsync(int id) =>
        await _context.Tenants
            .Include(t => t.Leases).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .Include(t => t.Leases).ThenInclude(l => l.Payments)
            .Include(t => t.MaintenanceRequests).ThenInclude(m => m.Unit)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task CreateAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Tenant created: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var tenant = await _context.Tenants.Include(t => t.Leases).FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return (false, "Tenant not found.");

        if (tenant.Leases.Any(l => l.Status == LeaseStatus.Active))
            return (false, "Cannot deactivate tenant with active leases. Please terminate or expire active leases first.");

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Tenant deactivated: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
        return (true, null);
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync() =>
        await _context.Tenants.Where(t => t.IsActive).OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToListAsync();
}
