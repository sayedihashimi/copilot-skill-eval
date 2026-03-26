using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Leases;

public class EditModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public EditModel(ILeaseService leaseService) => _leaseService = leaseService;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public string TenantName { get; set; } = "";
    public string UnitInfo { get; set; } = "";

    public class InputModel
    {
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent")] public decimal MonthlyRentAmount { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount")] public decimal DepositAmount { get; set; }
        [MaxLength(2000)] public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetByIdAsync(id);
        if (lease == null) return NotFound();
        Id = id;
        TenantName = lease.Tenant.FullName;
        UnitInfo = $"{lease.Unit.Property.Name} — Unit {lease.Unit.UnitNumber}";
        Input = new InputModel { MonthlyRentAmount = lease.MonthlyRentAmount, DepositAmount = lease.DepositAmount, Notes = lease.Notes };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var l = await _leaseService.GetByIdAsync(Id);
            TenantName = l?.Tenant.FullName ?? "";
            UnitInfo = l != null ? $"{l.Unit.Property.Name} — Unit {l.Unit.UnitNumber}" : "";
            return Page();
        }

        var lease = await _leaseService.GetByIdAsync(Id);
        if (lease == null) return NotFound();

        lease.MonthlyRentAmount = Input.MonthlyRentAmount;
        lease.DepositAmount = Input.DepositAmount;
        lease.Notes = Input.Notes;

        await _leaseService.UpdateAsync(lease);
        TempData["SuccessMessage"] = "Lease updated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
