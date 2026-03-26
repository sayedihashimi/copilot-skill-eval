using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Employees;

public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    public PaginatedList<Employee> Employees { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmploymentType? EmploymentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public EmployeeStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public SelectList DepartmentList { get; set; } = default!;

    public async Task OnGetAsync()
    {
        var departments = await _departmentService.GetAllFlatAsync();
        DepartmentList = new SelectList(departments, "Id", "Name");

        Employees = await _employeeService.GetAllAsync(
            PageNumber,
            pageSize: 10,
            search: Search,
            departmentId: DepartmentId,
            employmentType: EmploymentType,
            status: Status);
    }
}
