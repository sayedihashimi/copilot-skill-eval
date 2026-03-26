using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public IndexModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public List<Event> UpcomingEvents { get; set; } = new();
    public List<Event> TodaysEvents { get; set; } = new();
    public List<Registration> RecentRegistrations { get; set; } = new();
    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int EventsThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        UpcomingEvents = await _eventService.GetUpcomingEventsAsync(7);
        TodaysEvents = await _eventService.GetTodaysEventsAsync();
        RecentRegistrations = await _registrationService.GetRecentRegistrationsAsync(10);
        TotalEvents = await _eventService.GetTotalEventsCountAsync();
        TotalRegistrations = await _eventService.GetTotalRegistrationsCountAsync();
        EventsThisMonth = await _eventService.GetEventsThisMonthCountAsync();
    }
}
