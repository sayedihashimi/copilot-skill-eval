using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Departments;

public class CreateModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(IDepartmentService departmentService, IEmployeeService employeeService)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ParentDepartmentOptions { get; set; } = default!;
    public SelectList ManagerOptions { get; set; } = default!;

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Parent Department")]
        public int? ParentDepartmentId { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await PopulateDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        var department = new Department
        {
            Name = Input.Name,
            Code = Input.Code,
            Description = Input.Description,
            ParentDepartmentId = Input.ParentDepartmentId,
            ManagerId = Input.ManagerId
        };

        await _departmentService.CreateAsync(department);

        TempData["SuccessMessage"] = $"Department '{department.Name}' was created successfully.";
        return RedirectToPage("Index");
    }

    private async Task PopulateDropdownsAsync()
    {
        var departments = await _departmentService.GetAllFlatAsync();
        ParentDepartmentOptions = new SelectList(departments, "Id", "Name");

        var employees = await _employeeService.GetAllActiveAsync();
        ManagerOptions = new SelectList(employees, "Id", "FullName");
    }
}
