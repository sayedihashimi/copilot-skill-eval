using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Reviews;

public class SelfAssessmentModel : PageModel
{
    private readonly IReviewService _reviewService;

    public SelfAssessmentModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview Review { get; set; } = null!;

    [BindProperty]
    [Required(ErrorMessage = "Self-assessment is required.")]
    [MaxLength(5000, ErrorMessage = "Self-assessment cannot exceed 5000 characters.")]
    [Display(Name = "Self-Assessment")]
    public string SelfAssessment { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.Status != ReviewStatus.SelfAssessmentPending)
        {
            TempData["ErrorMessage"] = "Self-assessment can only be submitted when the review status is Self-Assessment Pending.";
            return RedirectToPage("Details", new { id });
        }

        Review = review;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.Status != ReviewStatus.SelfAssessmentPending)
        {
            TempData["ErrorMessage"] = "Self-assessment can only be submitted when the review status is Self-Assessment Pending.";
            return RedirectToPage("Details", new { id });
        }

        if (!ModelState.IsValid)
        {
            Review = review;
            return Page();
        }

        await _reviewService.SubmitSelfAssessmentAsync(id, SelfAssessment);
        TempData["SuccessMessage"] = "Self-assessment submitted successfully.";
        return RedirectToPage("Details", new { id });
    }
}
