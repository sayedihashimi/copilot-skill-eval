using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Units;

public class IndexModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IUnitService unitService, IPropertyService propertyService)
    {
        _unitService = unitService;
        _propertyService = propertyService;
    }

    public PaginatedList<Unit> Units { get; set; } = null!;
    public List<Property> PropertyList { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public UnitStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? Bedrooms { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? MinRent { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? MaxRent { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var allProperties = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        PropertyList = allProperties.Items;
        Units = await _unitService.GetUnitsAsync(PropertyId, Status, Bedrooms, MinRent, MaxRent, Search, PageNumber, 10);
    }
}
