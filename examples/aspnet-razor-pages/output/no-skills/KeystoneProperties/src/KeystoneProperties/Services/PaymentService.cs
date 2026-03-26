using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .AsQueryable();

        if (type.HasValue) query = query.Where(p => p.PaymentType == type.Value);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value);
        if (propertyId.HasValue) query = query.Where(p => p.Lease.Unit.PropertyId == propertyId.Value);

        query = query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Payment>(items, count, pageNumber, pageSize);
    }

    public async Task<Payment?> GetByIdAsync(int id) =>
        await _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Payment?> GetWithDetailsAsync(int id) => await GetByIdAsync(id);

    public async Task<(bool Success, string? Error)> RecordPaymentAsync(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        _context.Payments.Add(payment);

        // Late fee calculation for rent payments
        if (payment.PaymentType == PaymentType.Rent && payment.Status == PaymentStatus.Completed)
        {
            int daysLate = payment.PaymentDate.DayNumber - payment.DueDate.DayNumber;
            if (daysLate > 5)
            {
                decimal lateFee = Math.Min(50m + (daysLate - 5) * 5m, 200m);
                var lateFeePayment = new Payment
                {
                    LeaseId = payment.LeaseId,
                    Amount = lateFee,
                    PaymentDate = payment.PaymentDate,
                    DueDate = payment.DueDate,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = PaymentType.LateFee,
                    Status = PaymentStatus.Completed,
                    ReferenceNumber = $"LF-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Notes = $"Late fee: {daysLate} days past due date. $50 base + ${(daysLate - 5) * 5} additional (capped at $200).",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(lateFeePayment);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Payment recorded: {PaymentType} of {Amount} for Lease {LeaseId}", payment.PaymentType, payment.Amount, payment.LeaseId);
        return (true, null);
    }

    public async Task<List<OverdueLeaseInfo>> GetOverduePaymentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeLeases = await _context.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Payments)
            .Where(l => l.Status == LeaseStatus.Active)
            .ToListAsync();

        var overdueList = new List<OverdueLeaseInfo>();

        foreach (var lease in activeLeases)
        {
            // Check each month from start to current
            var currentMonth = lease.StartDate;
            while (currentMonth <= today && currentMonth <= lease.EndDate)
            {
                var hasPayment = lease.Payments.Any(p =>
                    p.PaymentType == PaymentType.Rent &&
                    p.Status == PaymentStatus.Completed &&
                    p.DueDate.Year == currentMonth.Year &&
                    p.DueDate.Month == currentMonth.Month);

                if (!hasPayment && currentMonth < today)
                {
                    overdueList.Add(new OverdueLeaseInfo
                    {
                        Lease = lease,
                        DueDate = currentMonth,
                        DaysOverdue = today.DayNumber - currentMonth.DayNumber,
                        AmountDue = lease.MonthlyRentAmount
                    });
                }

                currentMonth = currentMonth.AddMonths(1);
            }
        }

        return overdueList.OrderByDescending(o => o.DaysOverdue).ToList();
    }

    public async Task<decimal> GetRentCollectedThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateOnly(now.Year, now.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return await _context.Payments
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= firstOfMonth &&
                        p.PaymentDate <= lastOfMonth)
            .SumAsync(p => p.Amount);
    }
}
