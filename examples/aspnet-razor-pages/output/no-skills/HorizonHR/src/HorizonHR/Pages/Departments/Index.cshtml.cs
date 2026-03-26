using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public List<Department> Departments { get; set; } = new();
    public List<(Department Department, int Level)> FlattenedDepartments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetHierarchyAsync();
        FlattenedDepartments = new List<(Department, int)>();
        FlattenHierarchy(Departments, 0);
    }

    private void FlattenHierarchy(IEnumerable<Department> departments, int level)
    {
        foreach (var dept in departments)
        {
            FlattenedDepartments.Add((dept, level));
            if (dept.ChildDepartments.Any())
            {
                FlattenHierarchy(dept.ChildDepartments, level + 1);
            }
        }
    }
}
