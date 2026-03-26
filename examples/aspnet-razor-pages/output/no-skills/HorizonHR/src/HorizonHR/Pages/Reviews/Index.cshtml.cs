using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(IReviewService reviewService, IDepartmentService departmentService)
    {
        _reviewService = reviewService;
        _departmentService = departmentService;
    }

    public PaginatedList<PerformanceReview> Reviews { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public ReviewStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public OverallRating? Rating { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    public SelectList StatusOptions { get; set; } = null!;
    public SelectList RatingOptions { get; set; } = null!;
    public SelectList DepartmentOptions { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadFilterOptionsAsync();
        Reviews = await _reviewService.GetAllAsync(PageNumber, 10, Status, Rating, DepartmentId);
    }

    private async Task LoadFilterOptionsAsync()
    {
        StatusOptions = new SelectList(
            Enum.GetValues<ReviewStatus>().Select(s => new { Value = s.ToString(), Text = System.Text.RegularExpressions.Regex.Replace(s.ToString(), "([a-z])([A-Z])", "$1 $2") }),
            "Value", "Text", Status?.ToString());

        RatingOptions = new SelectList(
            Enum.GetValues<OverallRating>().Select(r => new { Value = r.ToString(), Text = System.Text.RegularExpressions.Regex.Replace(r.ToString(), "([a-z])([A-Z])", "$1 $2") }),
            "Value", "Text", Rating?.ToString());

        var departments = await _departmentService.GetAllFlatAsync();
        DepartmentOptions = new SelectList(departments, "Id", "Name", DepartmentId);
    }
}
