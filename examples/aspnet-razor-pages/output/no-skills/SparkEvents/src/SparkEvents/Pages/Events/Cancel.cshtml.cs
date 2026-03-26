using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CancelModel : PageModel
{
    private readonly IEventService _eventService;

    public CancelModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event? Event { get; set; }
    public int RegistrationCount { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Cancellation reason is required.")]
    public string CancellationReason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetByIdWithDetailsAsync(id);
        if (Event == null) return NotFound();

        RegistrationCount = Event.Registrations.Count(r => r.Status != RegistrationStatus.Cancelled);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            Event = await _eventService.GetByIdWithDetailsAsync(id);
            if (Event == null) return NotFound();
            RegistrationCount = Event.Registrations.Count(r => r.Status != RegistrationStatus.Cancelled);
            return Page();
        }

        var error = await _eventService.CancelEventAsync(id, CancellationReason);
        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event has been cancelled.";
        return RedirectToPage("Details", new { id });
    }
}
