using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Properties;

public class DeactivateModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DeactivateModel(IPropertyService propertyService) => _propertyService = propertyService;

    public Property? Property { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Property = await _propertyService.GetByIdAsync(id);
        if (Property == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await _propertyService.DeactivateAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Property deactivated successfully.";
        return RedirectToPage("Index");
    }
}
