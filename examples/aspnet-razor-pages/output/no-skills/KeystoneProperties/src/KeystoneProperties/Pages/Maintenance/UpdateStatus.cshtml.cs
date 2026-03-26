using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Maintenance;

public class UpdateStatusModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    public UpdateStatusModel(IMaintenanceService maintenanceService) => _maintenanceService = maintenanceService;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public new MaintenanceRequest? Request { get; set; }
    public List<MaintenanceStatus> ValidTransitions{ get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "New Status")] public MaintenanceStatus NewStatus { get; set; }
        [MaxLength(200), Display(Name = "Assigned To")] public string? AssignedTo { get; set; }
        [MaxLength(2000), Display(Name = "Completion Notes")] public string? CompletionNotes { get; set; }
        [Display(Name = "Estimated Cost"), DataType(DataType.Currency)] public decimal? EstimatedCost { get; set; }
        [Display(Name = "Actual Cost"), DataType(DataType.Currency)] public decimal? ActualCost { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Request = await _maintenanceService.GetWithDetailsAsync(id);
        if (Request == null) return NotFound();
        Id = id;
        Input.AssignedTo = Request.AssignedTo;
        SetValidTransitions(Request.Status);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Request = await _maintenanceService.GetWithDetailsAsync(Id);
        if (Request == null) return NotFound();

        if (!ModelState.IsValid) { SetValidTransitions(Request.Status); return Page(); }

        var (success, error) = await _maintenanceService.UpdateStatusAsync(Id, Input.NewStatus, Input.AssignedTo, Input.CompletionNotes, Input.EstimatedCost, Input.ActualCost);
        if (!success) { TempData["ErrorMessage"] = error; SetValidTransitions(Request.Status); return Page(); }

        TempData["SuccessMessage"] = $"Maintenance request status updated to {Input.NewStatus}.";
        return RedirectToPage("Details", new { id = Id });
    }

    private void SetValidTransitions(MaintenanceStatus current)
    {
        ValidTransitions = current switch
        {
            MaintenanceStatus.Submitted => new() { MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled },
            MaintenanceStatus.Assigned => new() { MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled },
            MaintenanceStatus.InProgress => new() { MaintenanceStatus.Completed, MaintenanceStatus.Cancelled },
            _ => new()
        };
    }
}
