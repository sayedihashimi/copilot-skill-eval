using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Departments;

public class DetailsModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public DetailsModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public Department Department { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        Department = department;
        return Page();
    }
}
