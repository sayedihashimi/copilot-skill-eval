using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Reviews;

public class ManagerReviewModel : PageModel
{
    private readonly IReviewService _reviewService;

    public ManagerReviewModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview Review { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList RatingOptions { get; set; } = null!;

    public class InputModel
    {
        [Required(ErrorMessage = "Manager assessment is required.")]
        [MaxLength(5000, ErrorMessage = "Manager assessment cannot exceed 5000 characters.")]
        [Display(Name = "Manager Assessment")]
        public string ManagerAssessment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an overall rating.")]
        [Display(Name = "Overall Rating")]
        public OverallRating OverallRating { get; set; }

        [MaxLength(2000, ErrorMessage = "Strengths cannot exceed 2000 characters.")]
        [Display(Name = "Strengths Noted")]
        public string? StrengthsNoted { get; set; }

        [MaxLength(2000, ErrorMessage = "Areas for improvement cannot exceed 2000 characters.")]
        [Display(Name = "Areas for Improvement")]
        public string? AreasForImprovement { get; set; }

        [MaxLength(5000, ErrorMessage = "Goals cannot exceed 5000 characters.")]
        [Display(Name = "Goals")]
        public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.Status != ReviewStatus.ManagerReviewPending)
        {
            TempData["ErrorMessage"] = "Manager review can only be completed when the review status is Manager Review Pending.";
            return RedirectToPage("Details", new { id });
        }

        Review = review;
        LoadRatingOptions();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.Status != ReviewStatus.ManagerReviewPending)
        {
            TempData["ErrorMessage"] = "Manager review can only be completed when the review status is Manager Review Pending.";
            return RedirectToPage("Details", new { id });
        }

        if (!ModelState.IsValid)
        {
            Review = review;
            LoadRatingOptions();
            return Page();
        }

        await _reviewService.CompleteManagerReviewAsync(
            id,
            Input.ManagerAssessment,
            Input.OverallRating,
            Input.StrengthsNoted,
            Input.AreasForImprovement,
            Input.Goals);

        TempData["SuccessMessage"] = "Manager review completed successfully.";
        return RedirectToPage("Details", new { id });
    }

    private void LoadRatingOptions()
    {
        RatingOptions = new SelectList(
            Enum.GetValues<OverallRating>().Select(r => new
            {
                Value = r.ToString(),
                Text = System.Text.RegularExpressions.Regex.Replace(r.ToString(), "([a-z])([A-Z])", "$1 $2")
            }),
            "Value", "Text");
    }
}
