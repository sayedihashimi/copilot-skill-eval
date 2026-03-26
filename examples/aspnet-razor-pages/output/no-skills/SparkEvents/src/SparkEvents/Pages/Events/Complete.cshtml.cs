using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CompleteModel : PageModel
{
    private readonly IEventService _eventService;

    public CompleteModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event? Event { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetByIdAsync(id);
        if (Event == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var error = await _eventService.CompleteEventAsync(id);
        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event marked as completed.";
        return RedirectToPage("Details", new { id });
    }
}
