using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Employees;

public class TerminateModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly IDepartmentService _departmentService;

    public TerminateModel(
        IEmployeeService employeeService,
        ILeaveService leaveService,
        IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _departmentService = departmentService;
    }

    public Employee Employee { get; set; } = default!;
    public List<LeaveRequest> PendingLeaveRequests { get; set; } = new();
    public List<Department> ManagedDepartments { get; set; } = new();
    public List<Employee> DirectReports { get; set; } = new();

    [BindProperty, Required, Display(Name = "Termination Date")]
    public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (employee.Status == EmployeeStatus.Terminated)
        {
            TempData["ErrorMessage"] = "This employee is already terminated.";
            return RedirectToPage("Details", new { id });
        }

        Employee = employee;
        await LoadCascadingEffects(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (employee.Status == EmployeeStatus.Terminated)
        {
            TempData["ErrorMessage"] = "This employee is already terminated.";
            return RedirectToPage("Details", new { id });
        }

        if (!ModelState.IsValid)
        {
            Employee = employee;
            await LoadCascadingEffects(id);
            return Page();
        }

        await _employeeService.TerminateAsync(id, TerminationDate);

        TempData["SuccessMessage"] = $"Employee {employee.FullName} has been terminated effective {TerminationDate:MMMM d, yyyy}.";
        return RedirectToPage("Details", new { id });
    }

    private async Task LoadCascadingEffects(int employeeId)
    {
        var leaveRequests = await _leaveService.GetEmployeeRequestsAsync(employeeId);
        PendingLeaveRequests = leaveRequests
            .Where(lr => lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved)
            .ToList();

        DirectReports = await _employeeService.GetDirectReportsAsync(employeeId);

        var allDepartments = await _departmentService.GetAllFlatAsync();
        ManagedDepartments = allDepartments.Where(d => d.ManagerId == employeeId).ToList();
    }
}
