using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class DetailsModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public DetailsModel(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public LeaveBalance? CurrentBalance { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _leaveService.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        LeaveRequest = request;

        var balances = await _leaveService.GetEmployeeBalancesAsync(
            request.EmployeeId, request.StartDate.Year);
        CurrentBalance = balances.FirstOrDefault(b => b.LeaveTypeId == request.LeaveTypeId);

        return Page();
    }
}
