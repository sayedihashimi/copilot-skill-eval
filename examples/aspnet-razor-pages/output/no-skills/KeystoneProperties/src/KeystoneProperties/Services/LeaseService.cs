using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class LeaseService : ILeaseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LeaseService> _logger;

    public LeaseService(AppDbContext context, ILogger<LeaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Lease>> GetLeasesAsync(LeaseStatus? status, int? propertyId, int pageNumber, int pageSize)
    {
        var query = _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsQueryable();

        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (propertyId.HasValue) query = query.Where(l => l.Unit.PropertyId == propertyId.Value);

        query = query.OrderByDescending(l => l.StartDate);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Lease>(items, count, pageNumber, pageSize);
    }

    public async Task<Lease?> GetByIdAsync(int id) =>
        await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Lease?> GetWithDetailsAsync(int id) =>
        await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .Include(l => l.Inspections).ThenInclude(i => i.Unit)
            .Include(l => l.RenewalOfLease)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<(bool Success, string? Error)> CreateAsync(Lease lease)
    {
        // Validate no overlapping leases
        var overlapping = await _context.Leases.AnyAsync(l =>
            l.UnitId == lease.UnitId &&
            (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
            l.StartDate < lease.EndDate && l.EndDate > lease.StartDate);

        if (overlapping)
            return (false, "An active or pending lease already exists for this unit during the specified dates.");

        if (lease.EndDate <= lease.StartDate)
            return (false, "End date must be after start date.");

        lease.CreatedAt = DateTime.UtcNow;
        lease.UpdatedAt = DateTime.UtcNow;
        _context.Leases.Add(lease);

        // Update unit status if lease is active
        if (lease.Status == LeaseStatus.Active)
        {
            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit != null) unit.Status = UnitStatus.Occupied;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease created: ID {LeaseId} for Unit {UnitId}, Tenant {TenantId}", lease.Id, lease.UnitId, lease.TenantId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Lease lease)
    {
        lease.UpdatedAt = DateTime.UtcNow;
        _context.Leases.Update(lease);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> TerminateAsync(int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount)
    {
        var lease = await _context.Leases.Include(l => l.Unit).FirstOrDefaultAsync(l => l.Id == id);
        if (lease == null) return (false, "Lease not found.");

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
            return (false, "Only active or pending leases can be terminated.");

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;
        lease.UpdatedAt = DateTime.UtcNow;

        // Create deposit return payment if applicable
        if ((depositStatus == DepositStatus.Returned || depositStatus == DepositStatus.PartiallyReturned) && depositReturnAmount.HasValue && depositReturnAmount.Value > 0)
        {
            var returnPayment = new Payment
            {
                LeaseId = lease.Id,
                Amount = depositReturnAmount.Value,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = PaymentMethod.Check,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit return upon lease termination. Reason: {reason}"
            };
            _context.Payments.Add(returnPayment);
        }

        // Update unit status
        var hasOtherActiveLease = await _context.Leases.AnyAsync(l => l.UnitId == lease.UnitId && l.Id != id && l.Status == LeaseStatus.Active);
        if (!hasOtherActiveLease)
            lease.Unit.Status = UnitStatus.Available;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease terminated: ID {LeaseId}. Reason: {Reason}", id, reason);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(int id, DateOnly newEndDate, decimal newMonthlyRent)
    {
        var originalLease = await _context.Leases.Include(l => l.Unit).FirstOrDefaultAsync(l => l.Id == id);
        if (originalLease == null) return (false, "Lease not found.", null);

        if (originalLease.Status != LeaseStatus.Active)
            return (false, "Only active leases can be renewed.", null);

        var newStartDate = originalLease.EndDate.AddDays(1);
        if (newEndDate <= newStartDate)
            return (false, "New end date must be after the new start date.", null);

        // Check for overlapping leases
        var overlapping = await _context.Leases.AnyAsync(l =>
            l.UnitId == originalLease.UnitId &&
            l.Id != id &&
            (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
            l.StartDate < newEndDate && l.EndDate > newStartDate);

        if (overlapping)
            return (false, "Another lease overlaps with the renewal period.", null);

        // Create new lease
        var newLease = new Lease
        {
            UnitId = originalLease.UnitId,
            TenantId = originalLease.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newMonthlyRent,
            DepositAmount = originalLease.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            RenewalOfLeaseId = originalLease.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        originalLease.Status = LeaseStatus.Renewed;
        originalLease.UpdatedAt = DateTime.UtcNow;

        _context.Leases.Add(newLease);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease renewed: Original ID {OriginalId}, New ID {NewId}", id, newLease.Id);
        return (true, null, newLease);
    }

    public async Task<List<Lease>> GetUpcomingExpirationsAsync(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(days);
        return await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= cutoff)
            .OrderBy(l => l.EndDate)
            .ToListAsync();
    }

    public async Task<List<Lease>> GetActiveLeasesForUnitAsync(int unitId) =>
        await _context.Leases
            .Include(l => l.Tenant)
            .Where(l => l.UnitId == unitId && l.Status == LeaseStatus.Active)
            .ToListAsync();
}
