using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Leases;

public class DetailsModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public DetailsModel(ILeaseService leaseService) => _leaseService = leaseService;

    public Lease? Lease { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Lease = await _leaseService.GetWithDetailsAsync(id);
        if (Lease == null) return NotFound();
        return Page();
    }
}
