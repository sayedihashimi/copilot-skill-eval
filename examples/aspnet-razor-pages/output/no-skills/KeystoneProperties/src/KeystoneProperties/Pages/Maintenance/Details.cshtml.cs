using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Maintenance;

public class DetailsModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    public DetailsModel(IMaintenanceService maintenanceService) => _maintenanceService = maintenanceService;

    public new MaintenanceRequest? Request { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Request = await _maintenanceService.GetWithDetailsAsync(id);
        if (Request == null) return NotFound();
        return Page();
    }
}
