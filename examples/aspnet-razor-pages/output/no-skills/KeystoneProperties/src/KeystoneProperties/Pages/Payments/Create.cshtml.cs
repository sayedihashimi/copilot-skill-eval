using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Payments;

public class CreateModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly AppDbContext _context;

    public CreateModel(IPaymentService paymentService, AppDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Lease> LeaseList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Lease")] public int LeaseId { get; set; }
        [Required, Range(0.01, double.MaxValue), DataType(DataType.Currency)] public decimal Amount { get; set; }
        [Required, Display(Name = "Payment Date")] public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        [Required, Display(Name = "Due Date")] public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        [Required, Display(Name = "Payment Method")] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
        [Required, Display(Name = "Payment Type")] public PaymentType PaymentType { get; set; } = PaymentType.Rent;
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        [MaxLength(100), Display(Name = "Reference Number")] public string? ReferenceNumber { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        LeaseList = await _context.Leases
            .Include(l => l.Tenant).Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active)
            .OrderBy(l => l.Tenant.LastName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LeaseList = await _context.Leases.Include(l => l.Tenant).Include(l => l.Unit).ThenInclude(u => u.Property)
                .Where(l => l.Status == LeaseStatus.Active).OrderBy(l => l.Tenant.LastName).ToListAsync();
            return Page();
        }

        var payment = new Payment
        {
            LeaseId = Input.LeaseId, Amount = Input.Amount,
            PaymentDate = Input.PaymentDate, DueDate = Input.DueDate,
            PaymentMethod = Input.PaymentMethod, PaymentType = Input.PaymentType,
            Status = Input.Status, ReferenceNumber = Input.ReferenceNumber, Notes = Input.Notes
        };

        var (success, error) = await _paymentService.RecordPaymentAsync(payment);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            LeaseList = await _context.Leases.Include(l => l.Tenant).Include(l => l.Unit).ThenInclude(u => u.Property)
                .Where(l => l.Status == LeaseStatus.Active).OrderBy(l => l.Tenant.LastName).ToListAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Payment recorded successfully." +
            (Input.PaymentType == PaymentType.Rent && Input.PaymentDate.DayNumber - Input.DueDate.DayNumber > 5
                ? " A late fee was also generated." : "");
        return RedirectToPage("Details", new { id = payment.Id });
    }
}
