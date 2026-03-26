using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Leases;

public class IndexModel : PageModel
{
    private readonly ILeaseService _leaseService;
    private readonly IPropertyService _propertyService;

    public IndexModel(ILeaseService leaseService, IPropertyService propertyService)
    {
        _leaseService = leaseService;
        _propertyService = propertyService;
    }

    public PaginatedList<Lease> Leases { get; set; } = null!;
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public LeaseStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var props = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        PropertyList = props.Items;
        Leases = await _leaseService.GetLeasesAsync(Status, PropertyId, PageNumber, 10);
    }
}
