using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class CancelModel : PageModel
{
    private readonly IRegistrationService _registrationService;

    public CancelModel(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Registration? Registration { get; set; }

    [BindProperty]
    public string? CancellationReason { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Registration = await _registrationService.GetByIdAsync(id);
        if (Registration == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var error = await _registrationService.CancelRegistrationAsync(id, CancellationReason);
        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Registration cancelled successfully.";
        return RedirectToPage("Details", new { id });
    }
}
