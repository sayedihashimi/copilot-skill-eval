using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Maintenance;

public class IndexModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IMaintenanceService maintenanceService, IPropertyService propertyService)
    {
        _maintenanceService = maintenanceService;
        _propertyService = propertyService;
    }

    public PaginatedList<MaintenanceRequest> Requests { get; set; } = null!;
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public MaintenanceStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public MaintenancePriority? Priority { get; set; }
    [BindProperty(SupportsGet = true)] public MaintenanceCategory? Category { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var props = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        PropertyList = props.Items;
        Requests = await _maintenanceService.GetRequestsAsync(Status, Priority, PropertyId, Category, PageNumber, 10);
    }
}
