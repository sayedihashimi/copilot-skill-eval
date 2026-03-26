using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Inspections;

public class DetailsModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    public DetailsModel(IInspectionService inspectionService) => _inspectionService = inspectionService;

    public Inspection? Inspection { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (Inspection == null) return NotFound();
        return Page();
    }
}
