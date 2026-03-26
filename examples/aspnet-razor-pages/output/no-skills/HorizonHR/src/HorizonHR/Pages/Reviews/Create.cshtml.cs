using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Reviews;

public class CreateModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(IReviewService reviewService, IEmployeeService employeeService)
    {
        _reviewService = reviewService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList EmployeeOptions { get; set; } = null!;
    public SelectList ReviewerOptions { get; set; } = null!;

    public class InputModel
    {
        [Required(ErrorMessage = "Please select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please select a reviewer.")]
        [Display(Name = "Reviewer")]
        public int ReviewerId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Review Period Start")]
        public DateOnly ReviewPeriodStart { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "Review Period End")]
        public DateOnly ReviewPeriodEnd { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadEmployeeOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.ReviewPeriodEnd <= Input.ReviewPeriodStart)
        {
            ModelState.AddModelError("Input.ReviewPeriodEnd", "End date must be after start date.");
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeeOptionsAsync();
            return Page();
        }

        var review = new PerformanceReview
        {
            EmployeeId = Input.EmployeeId,
            ReviewerId = Input.ReviewerId,
            ReviewPeriodStart = Input.ReviewPeriodStart,
            ReviewPeriodEnd = Input.ReviewPeriodEnd
        };

        try
        {
            await _reviewService.CreateAsync(review);
            TempData["SuccessMessage"] = "Performance review created successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadEmployeeOptionsAsync();
            return Page();
        }
    }

    private async Task LoadEmployeeOptionsAsync()
    {
        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = new SelectList(
            employees.Select(e => new { e.Id, Name = e.FullName }).OrderBy(e => e.Name),
            "Id", "Name", Input.EmployeeId);
        ReviewerOptions = new SelectList(
            employees.Select(e => new { e.Id, Name = e.FullName }).OrderBy(e => e.Name),
            "Id", "Name", Input.ReviewerId);
    }
}
