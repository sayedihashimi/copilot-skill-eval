using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class EmployeeModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public EmployeeModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = null!;
    public List<LeaveBalance> Balances { get; set; } = new();
    public List<LeaveRequest> Requests { get; set; } = new();
    public int CurrentYear { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Employee = employee;
        CurrentYear = DateTime.UtcNow.Year;
        Balances = await _leaveService.GetEmployeeBalancesAsync(id, CurrentYear);
        Requests = await _leaveService.GetEmployeeRequestsAsync(id);

        return Page();
    }
}
