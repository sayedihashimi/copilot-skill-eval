using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Leases;

public class RenewModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public RenewModel(ILeaseService leaseService) => _leaseService = leaseService;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public Lease? Lease { get; set; }

    public class InputModel
    {
        [Required, Display(Name = "New End Date")] public DateOnly NewEndDate { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent"), DataType(DataType.Currency)] public decimal NewMonthlyRent { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Lease = await _leaseService.GetByIdAsync(id);
        if (Lease == null) return NotFound();
        Id = id;
        Input.NewEndDate = Lease.EndDate.AddDays(1).AddMonths(12);
        Input.NewMonthlyRent = Lease.MonthlyRentAmount;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Lease = await _leaseService.GetByIdAsync(Id);
            return Page();
        }

        var (success, error, newLease) = await _leaseService.RenewAsync(Id, Input.NewEndDate, Input.NewMonthlyRent);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            Lease = await _leaseService.GetByIdAsync(Id);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease renewed successfully.";
        return RedirectToPage("Details", new { id = newLease!.Id });
    }
}
