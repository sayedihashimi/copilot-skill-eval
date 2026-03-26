using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Pages.Shared;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RosterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public RosterModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public Event? Event { get; set; }
    public List<Registration> Registrations { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetByIdAsync(eventId);
        if (Event == null) return NotFound();

        var (items, total) = await _registrationService.GetEventRosterAsync(eventId, Search, PageNumber, 10);
        Registrations = items;

        Pagination = new PaginationModel(PageNumber, total, 10,
            $"/Events/{eventId}/Roster",
            new Dictionary<string, string?> { ["search"] = Search });

        return Page();
    }
}
