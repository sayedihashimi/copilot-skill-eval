using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly IEventService _eventService;

    public DetailsModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event? Event { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetByIdWithDetailsAsync(id);
        if (Event == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        var error = await _eventService.PublishAsync(id);
        if (error != null)
        {
            TempData["ErrorMessage"] = error;
        }
        else
        {
            TempData["SuccessMessage"] = "Event published successfully.";
        }
        return RedirectToPage("Details", new { id });
    }
}
