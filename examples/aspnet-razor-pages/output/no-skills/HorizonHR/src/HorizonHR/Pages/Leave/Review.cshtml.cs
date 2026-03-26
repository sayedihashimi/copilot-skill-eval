using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class ReviewModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public ReviewModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public LeaveBalance? CurrentBalance { get; set; }
    public SelectList ReviewerOptions { get; set; } = null!;

    [BindProperty]
    [Required(ErrorMessage = "Please select a reviewer.")]
    [Display(Name = "Reviewed By")]
    public int ReviewedById { get; set; }

    [BindProperty]
    [MaxLength(1000)]
    [Display(Name = "Review Notes")]
    public string? ReviewNotes { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadRequestAsync(id);
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await LoadRequestAsync(id);
            return Page();
        }

        try
        {
            await _leaveService.ApproveAsync(id, ReviewedById, ReviewNotes);
            TempData["SuccessMessage"] = "Leave request has been approved.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadRequestAsync(id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await LoadRequestAsync(id);
            return Page();
        }

        try
        {
            await _leaveService.RejectAsync(id, ReviewedById, ReviewNotes);
            TempData["SuccessMessage"] = "Leave request has been rejected.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadRequestAsync(id);
            return Page();
        }
    }

    private async Task<IActionResult> LoadRequestAsync(int id)
    {
        var request = await _leaveService.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != LeaveRequestStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Only submitted requests can be reviewed.";
            return RedirectToPage("Details", new { id });
        }

        LeaveRequest = request;

        var balances = await _leaveService.GetEmployeeBalancesAsync(
            request.EmployeeId, request.StartDate.Year);
        CurrentBalance = balances.FirstOrDefault(b => b.LeaveTypeId == request.LeaveTypeId);

        var employees = await _employeeService.GetAllActiveAsync();
        ReviewerOptions = new SelectList(employees, "Id", "FullName", ReviewedById);

        return Page();
    }
}
