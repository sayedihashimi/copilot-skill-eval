using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class CancelModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public CancelModel(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public bool WasApproved { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _leaveService.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != LeaveRequestStatus.Submitted && request.Status != LeaveRequestStatus.Approved)
        {
            TempData["ErrorMessage"] = "Only submitted or approved requests can be cancelled.";
            return RedirectToPage("Details", new { id });
        }

        LeaveRequest = request;
        WasApproved = request.Status == LeaveRequestStatus.Approved;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _leaveService.CancelAsync(id);
            TempData["SuccessMessage"] = "Leave request has been cancelled.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
