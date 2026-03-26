using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Services;

public class DashboardService : IDashboardService
{
    private readonly IPropertyService _propertyService;
    private readonly ILeaseService _leaseService;
    private readonly IPaymentService _paymentService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly Data.AppDbContext _context;

    public DashboardService(
        IPropertyService propertyService,
        ILeaseService leaseService,
        IPaymentService paymentService,
        IMaintenanceService maintenanceService,
        Data.AppDbContext context)
    {
        _propertyService = propertyService;
        _leaseService = leaseService;
        _paymentService = paymentService;
        _maintenanceService = maintenanceService;
        _context = context;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        var totalProperties = _context.Properties.Count(p => p.IsActive);
        var totalUnits = _context.Units.Count();
        var occupiedUnits = _context.Units.Count(u => u.Status == Models.Enums.UnitStatus.Occupied);
        var occupancyRate = totalUnits > 0 ? (double)occupiedUnits / totalUnits * 100 : 0;

        return new DashboardStats
        {
            TotalProperties = totalProperties,
            TotalUnits = totalUnits,
            OccupancyRate = Math.Round(occupancyRate, 1),
            RentCollectedThisMonth = await _paymentService.GetRentCollectedThisMonthAsync(),
            OverduePaymentsCount = (await _paymentService.GetOverduePaymentsAsync()).Count,
            OpenMaintenanceCount = await _maintenanceService.GetOpenRequestCountAsync(),
            UpcomingExpirations = await _leaseService.GetUpcomingExpirationsAsync(30)
        };
    }
}
