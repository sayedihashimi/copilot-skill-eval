using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Inspections;

public class CreateModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    private readonly AppDbContext _context;

    public CreateModel(IInspectionService inspectionService, AppDbContext context)
    {
        _inspectionService = inspectionService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Unit> UnitList { get; set; } = new();
    public List<Lease> LeaseList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Unit")] public int UnitId { get; set; }
        [Required, Display(Name = "Type")] public InspectionType InspectionType { get; set; }
        [Required, Display(Name = "Scheduled Date")] public DateOnly ScheduledDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        [Required, MaxLength(200), Display(Name = "Inspector Name")] public string InspectorName { get; set; } = string.Empty;
        public int? LeaseId { get; set; }
        [MaxLength(5000)] public string? Notes { get; set; }
    }

    public async Task OnGetAsync() => await LoadListsAsync();

    private async Task LoadListsAsync()
    {
        UnitList = await _context.Units.Include(u => u.Property).OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber).ToListAsync();
        LeaseList = await _context.Leases.Include(l => l.Tenant)
            .Where(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending)
            .OrderByDescending(l => l.StartDate).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadListsAsync(); return Page(); }

        var inspection = new Inspection
        {
            UnitId = Input.UnitId, InspectionType = Input.InspectionType,
            ScheduledDate = Input.ScheduledDate, InspectorName = Input.InspectorName,
            LeaseId = Input.LeaseId, Notes = Input.Notes
        };

        await _inspectionService.CreateAsync(inspection);
        TempData["SuccessMessage"] = "Inspection scheduled successfully.";
        return RedirectToPage("Details", new { id = inspection.Id });
    }
}
