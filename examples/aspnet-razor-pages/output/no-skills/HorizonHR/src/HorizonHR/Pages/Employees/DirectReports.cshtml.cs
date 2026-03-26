using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Employees;

public class DirectReportsModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public DirectReportsModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = default!;
    public List<Employee> Reports { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        Employee = employee;
        Reports = await _employeeService.GetDirectReportsAsync(id);
        return Page();
    }
}
