using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DeleteModel : PageModel
{
    private readonly IVenueService _venueService;

    public DeleteModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public Venue? Venue { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venue = await _venueService.GetByIdAsync(id);
        if (Venue == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (await _venueService.HasFutureEventsAsync(id))
        {
            TempData["ErrorMessage"] = "Cannot delete a venue that has future events.";
            return RedirectToPage("Index");
        }

        var result = await _venueService.DeleteAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Could not delete venue.";
            return RedirectToPage("Index");
        }

        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToPage("Index");
    }
}
