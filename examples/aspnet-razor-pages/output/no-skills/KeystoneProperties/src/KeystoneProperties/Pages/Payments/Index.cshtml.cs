using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IPaymentService paymentService, IPropertyService propertyService)
    {
        _paymentService = paymentService;
        _propertyService = propertyService;
    }

    public PaginatedList<Payment> Payments { get; set; } = null!;
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public PaymentType? Type { get; set; }
    [BindProperty(SupportsGet = true)] public PaymentStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var props = await _propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        PropertyList = props.Items;
        Payments = await _paymentService.GetPaymentsAsync(Type, Status, FromDate, ToDate, PropertyId, PageNumber, 10);
    }
}
