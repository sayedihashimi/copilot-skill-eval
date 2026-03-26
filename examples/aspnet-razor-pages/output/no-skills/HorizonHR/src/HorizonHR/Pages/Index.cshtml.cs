using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages;

public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly IReviewService _reviewService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(
        IEmployeeService employeeService,
        ILeaveService leaveService,
        IReviewService reviewService,
        IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _reviewService = reviewService;
        _departmentService = departmentService;
    }

    public int TotalEmployees { get; set; }
    public int DepartmentCount { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int UpcomingReviews { get; set; }
    public List<Employee> RecentHires { get; set; } = new();
    public List<Employee> OnLeaveEmployees { get; set; } = new();
    public List<Department> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalEmployees = await _employeeService.GetTotalCountAsync();
        PendingLeaveRequests = await _leaveService.GetPendingCountAsync();
        UpcomingReviews = await _reviewService.GetUpcomingCountAsync();
        RecentHires = await _employeeService.GetRecentHiresAsync(30);
        OnLeaveEmployees = await _employeeService.GetOnLeaveAsync();
        Departments = await _departmentService.GetAllFlatAsync();
        DepartmentCount = Departments.Count;
    }
}
