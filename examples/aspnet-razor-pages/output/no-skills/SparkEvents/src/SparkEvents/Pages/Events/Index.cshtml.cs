using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Pages.Shared;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public IndexModel(IEventService eventService, ICategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    public List<Event> Events { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        CategoryOptions = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

        EventStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(Status) && Enum.TryParse<EventStatus>(Status, out var parsed))
            statusFilter = parsed;

        var filter = new EventFilterModel
        {
            Search = Search,
            CategoryId = CategoryId,
            Status = statusFilter,
            StartDate = StartDate,
            EndDate = EndDate,
            Page = PageNumber,
            PageSize = 10
        };

        var (items, total) = await _eventService.GetFilteredAsync(filter);
        Events = items;

        Pagination = new PaginationModel(PageNumber, total, 10, "/Events", new Dictionary<string, string?>
        {
            ["search"] = Search,
            ["categoryId"] = CategoryId?.ToString(),
            ["status"] = Status,
            ["startDate"] = StartDate?.ToString("yyyy-MM-dd"),
            ["endDate"] = EndDate?.ToString("yyyy-MM-dd")
        });
    }
}
