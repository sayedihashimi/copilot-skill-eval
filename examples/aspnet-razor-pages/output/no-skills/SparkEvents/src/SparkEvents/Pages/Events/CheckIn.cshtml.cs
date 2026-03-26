using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CheckInModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICheckInService _checkInService;

    public CheckInModel(IEventService eventService, ICheckInService checkInService)
    {
        _eventService = eventService;
        _checkInService = checkInService;
    }

    public Event? Event { get; set; }
    public List<Registration> Registrations { get; set; } = new();
    public int TotalCheckedIn { get; set; }
    public int TotalConfirmed { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetByIdAsync(eventId);
        if (Event == null) return NotFound();

        var (items, checkedIn, total) = await _checkInService.GetCheckInDashboardAsync(eventId, Search);
        Registrations = items;
        TotalCheckedIn = checkedIn;
        TotalConfirmed = total;

        return Page();
    }
}
