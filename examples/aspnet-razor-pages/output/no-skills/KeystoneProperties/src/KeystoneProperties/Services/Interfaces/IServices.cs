using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;

namespace KeystoneProperties.Services.Interfaces;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class DashboardStats
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public double OccupancyRate { get; set; }
    public decimal RentCollectedThisMonth { get; set; }
    public int OverduePaymentsCount { get; set; }
    public int OpenMaintenanceCount { get; set; }
    public List<Lease> UpcomingExpirations { get; set; } = new();
}

public interface IPropertyService
{
    Task<PaginatedList<Property>> GetPropertiesAsync(string? search, PropertyType? type, bool? isActive, int pageNumber, int pageSize);
    Task<Property?> GetByIdAsync(int id);
    Task<Property?> GetWithUnitsAsync(int id);
    Task CreateAsync(Property property);
    Task UpdateAsync(Property property);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<int> GetOccupiedUnitCountAsync(int propertyId);
}

public interface IUnitService
{
    Task<PaginatedList<Unit>> GetUnitsAsync(int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent, string? search, int pageNumber, int pageSize);
    Task<Unit?> GetByIdAsync(int id);
    Task<Unit?> GetWithDetailsAsync(int id);
    Task CreateAsync(Unit unit);
    Task UpdateAsync(Unit unit);
    Task<List<Unit>> GetAvailableUnitsAsync();
}

public interface ITenantService
{
    Task<PaginatedList<Tenant>> GetTenantsAsync(string? search, bool? isActive, int pageNumber, int pageSize);
    Task<Tenant?> GetByIdAsync(int id);
    Task<Tenant?> GetWithDetailsAsync(int id);
    Task CreateAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<List<Tenant>> GetActiveTenantsAsync();
}

public interface ILeaseService
{
    Task<PaginatedList<Lease>> GetLeasesAsync(LeaseStatus? status, int? propertyId, int pageNumber, int pageSize);
    Task<Lease?> GetByIdAsync(int id);
    Task<Lease?> GetWithDetailsAsync(int id);
    Task<(bool Success, string? Error)> CreateAsync(Lease lease);
    Task<(bool Success, string? Error)> UpdateAsync(Lease lease);
    Task<(bool Success, string? Error)> TerminateAsync(int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount);
    Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(int id, DateOnly newEndDate, decimal newMonthlyRent);
    Task<List<Lease>> GetUpcomingExpirationsAsync(int days);
    Task<List<Lease>> GetActiveLeasesForUnitAsync(int unitId);
}

public interface IPaymentService
{
    Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize);
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetWithDetailsAsync(int id);
    Task<(bool Success, string? Error)> RecordPaymentAsync(Payment payment);
    Task<List<OverdueLeaseInfo>> GetOverduePaymentsAsync();
    Task<decimal> GetRentCollectedThisMonthAsync();
}

public class OverdueLeaseInfo
{
    public Lease Lease { get; set; } = null!;
    public DateOnly DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal AmountDue { get; set; }
}

public interface IMaintenanceService
{
    Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize);
    Task<MaintenanceRequest?> GetByIdAsync(int id);
    Task<MaintenanceRequest?> GetWithDetailsAsync(int id);
    Task<(bool Success, string? Error)> CreateAsync(MaintenanceRequest request);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost);
    Task<int> GetOpenRequestCountAsync();
}

public interface IInspectionService
{
    Task<PaginatedList<Inspection>> GetInspectionsAsync(InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize);
    Task<Inspection?> GetByIdAsync(int id);
    Task<Inspection?> GetWithDetailsAsync(int id);
    Task CreateAsync(Inspection inspection);
    Task<(bool Success, string? Error)> CompleteAsync(int id, OverallCondition condition, string? notes, bool followUpRequired);
}

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync();
}
