using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Payments;

public class OverdueModel : PageModel
{
    private readonly IPaymentService _paymentService;
    public OverdueModel(IPaymentService paymentService) => _paymentService = paymentService;

    public List<OverdueLeaseInfo> OverduePayments { get; set; } = new();

    public async Task OnGetAsync()
    {
        OverduePayments = await _paymentService.GetOverduePaymentsAsync();
    }
}
