using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Leases;

public class TerminateModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public TerminateModel(ILeaseService leaseService) => _leaseService = leaseService;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public Lease? Lease { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Termination reason is required."), Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Deposit Disposition")]
        public DepositStatus DepositDisposition { get; set; } = DepositStatus.Held;

        [Display(Name = "Return Amount"), DataType(DataType.Currency)]
        public decimal? DepositReturnAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Lease = await _leaseService.GetByIdAsync(id);
        if (Lease == null) return NotFound();
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Lease = await _leaseService.GetByIdAsync(Id);
            return Page();
        }

        var (success, error) = await _leaseService.TerminateAsync(Id, Input.Reason, Input.DepositDisposition, Input.DepositReturnAmount);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id = Id });
        }

        TempData["SuccessMessage"] = "Lease terminated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
