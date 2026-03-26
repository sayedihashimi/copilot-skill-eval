using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Tenants;

public class DeactivateModel : PageModel
{
    private readonly ITenantService _tenantService;
    public DeactivateModel(ITenantService tenantService) => _tenantService = tenantService;

    public Tenant? Tenant { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tenant = await _tenantService.GetByIdAsync(id);
        if (Tenant == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await _tenantService.DeactivateAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }
        TempData["SuccessMessage"] = "Tenant deactivated successfully.";
        return RedirectToPage("Index");
    }
}
