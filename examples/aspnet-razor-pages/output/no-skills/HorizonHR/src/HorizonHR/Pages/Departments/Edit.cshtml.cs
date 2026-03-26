using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Departments;

public class EditModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;

    public EditModel(IDepartmentService departmentService, IEmployeeService employeeService)
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
        public int Id { get; set; }

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

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            ManagerId = department.ManagerId,
            IsActive = department.IsActive
        };

        await PopulateDropdownsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(Input.Id);
            return Page();
        }

        if (Input.ParentDepartmentId.HasValue &&
            await _departmentService.HasCircularReference(Input.Id, Input.ParentDepartmentId))
        {
            ModelState.AddModelError("Input.ParentDepartmentId",
                "The selected parent department would create a circular reference.");
            await PopulateDropdownsAsync(Input.Id);
            return Page();
        }

        var department = await _departmentService.GetByIdAsync(Input.Id);

        if (department == null)
        {
            return NotFound();
        }

        department.Name = Input.Name;
        department.Code = Input.Code;
        department.Description = Input.Description;
        department.ParentDepartmentId = Input.ParentDepartmentId;
        department.ManagerId = Input.ManagerId;
        department.IsActive = Input.IsActive;

        await _departmentService.UpdateAsync(department);

        TempData["SuccessMessage"] = $"Department '{department.Name}' was updated successfully.";
        return RedirectToPage("Index");
    }

    private async Task PopulateDropdownsAsync(int departmentId)
    {
        var departments = await _departmentService.GetAllFlatAsync();
        // Exclude the current department from parent options to prevent self-reference
        var parentOptions = departments.Where(d => d.Id != departmentId).ToList();
        ParentDepartmentOptions = new SelectList(parentOptions, "Id", "Name");

        var employees = await _employeeService.GetAllActiveAsync();
        ManagerOptions = new SelectList(employees, "Id", "FullName");
    }
}
