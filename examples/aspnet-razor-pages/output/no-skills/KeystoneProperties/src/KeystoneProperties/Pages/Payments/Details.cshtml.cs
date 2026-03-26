using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Payments;

public class DetailsModel : PageModel
{
    private readonly IPaymentService _paymentService;
    public DetailsModel(IPaymentService paymentService) => _paymentService = paymentService;

    public Payment? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Payment = await _paymentService.GetWithDetailsAsync(id);
        if (Payment == null) return NotFound();
        return Page();
    }
}
