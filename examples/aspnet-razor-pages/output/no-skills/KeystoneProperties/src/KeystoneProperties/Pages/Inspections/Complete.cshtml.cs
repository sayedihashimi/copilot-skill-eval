using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Inspections;

public class CompleteModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    public CompleteModel(IInspectionService inspectionService) => _inspectionService = inspectionService;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public Inspection? Inspection { get; set; }

    public class InputModel
    {
        [Required, Display(Name = "Overall Condition")] public OverallCondition OverallCondition { get; set; }
        [MaxLength(5000)] public string? Notes { get; set; }
        [Display(Name = "Follow-Up Required")] public bool FollowUpRequired { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (Inspection == null) return NotFound();
        Id = id;
        Input.Notes = Inspection.Notes;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { Inspection = await _inspectionService.GetWithDetailsAsync(Id); return Page(); }

        var (success, error) = await _inspectionService.CompleteAsync(Id, Input.OverallCondition, Input.Notes, Input.FollowUpRequired);
        if (!success) { TempData["ErrorMessage"] = error; return RedirectToPage("Details", new { id = Id }); }

        TempData["SuccessMessage"] = "Inspection completed successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
