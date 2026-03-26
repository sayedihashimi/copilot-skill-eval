using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Pages.Shared;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class IndexModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public IndexModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public List<Attendee> Attendees { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var (items, total) = await _attendeeService.GetPagedAsync(Search, PageNumber, 10);
        Attendees = items;

        Pagination = new PaginationModel(PageNumber, total, 10, "/Attendees",
            new Dictionary<string, string?> { ["search"] = Search });
    }
}
