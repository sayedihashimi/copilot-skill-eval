using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Tenants;

public class DetailsModel : PageModel
{
    private readonly ITenantService _tenantService;
    public DetailsModel(ITenantService tenantService) => _tenantService = tenantService;

    public Tenant? Tenant { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tenant = await _tenantService.GetWithDetailsAsync(id);
        if (Tenant == null) return NotFound();
        return Page();
    }
}
