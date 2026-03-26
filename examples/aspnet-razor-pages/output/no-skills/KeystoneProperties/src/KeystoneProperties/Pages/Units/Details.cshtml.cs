using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Units;

public class DetailsModel : PageModel
{
    private readonly IUnitService _unitService;
    public DetailsModel(IUnitService unitService) => _unitService = unitService;

    public Unit? Unit { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Unit = await _unitService.GetWithDetailsAsync(id);
        if (Unit == null) return NotFound();
        return Page();
    }
}
