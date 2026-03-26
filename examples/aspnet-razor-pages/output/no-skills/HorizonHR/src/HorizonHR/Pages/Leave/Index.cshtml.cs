using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class IndexModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public IndexModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public PaginatedList<LeaveRequest> LeaveRequests { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public LeaveRequestStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EmployeeId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeaveTypeId { get; set; }

    public SelectList StatusOptions { get; set; } = null!;
    public SelectList EmployeeOptions { get; set; } = null!;
    public SelectList LeaveTypeOptions { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await PopulateFiltersAsync();
        LeaveRequests = await _leaveService.GetAllRequestsAsync(PageNumber, 10, Status, EmployeeId, LeaveTypeId);
    }

    private async Task PopulateFiltersAsync()
    {
        StatusOptions = new SelectList(
            Enum.GetValues<LeaveRequestStatus>().Select(s => new { Value = s, Text = s.ToString() }),
            "Value", "Text", Status);

        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = new SelectList(employees, "Id", "FullName", EmployeeId);

        var leaveTypes = await _leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = new SelectList(leaveTypes, "Id", "Name", LeaveTypeId);
    }
}
