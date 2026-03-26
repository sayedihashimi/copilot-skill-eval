using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Properties;

public class DetailsModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DetailsModel(IPropertyService propertyService) => _propertyService = propertyService;

    public Property? Property { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Property = await _propertyService.GetWithUnitsAsync(id);
        if (Property == null) return NotFound();
        return Page();
    }
}
