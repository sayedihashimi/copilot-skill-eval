using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Employees;

public class EditModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public EditModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string EmployeeNumber { get; set; } = string.Empty;
    public SelectList DepartmentList { get; set; } = default!;
    public SelectList ManagerList { get; set; } = default!;

    public class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100), Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone, Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required, Display(Name = "Date of Birth")]
        public DateOnly DateOfBirth { get; set; }

        [Required, Display(Name = "Hire Date")]
        public DateOnly HireDate { get; set; }

        [Required, Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }

        [Required, MaxLength(200), Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required, Display(Name = "Employment Type")]
        public EmploymentType EmploymentType { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than zero.")]
        public decimal Salary { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        EmployeeNumber = employee.EmployeeNumber;
        Input = new InputModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId,
            ManagerId = employee.ManagerId,
            JobTitle = employee.JobTitle,
            EmploymentType = employee.EmploymentType,
            Salary = employee.Salary
        };

        await PopulateDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var existing = await _employeeService.GetByIdAsync(Input.Id);
            EmployeeNumber = existing?.EmployeeNumber ?? string.Empty;
            await PopulateDropdowns();
            return Page();
        }

        var employee = await _employeeService.GetByIdAsync(Input.Id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.FirstName = Input.FirstName;
        employee.LastName = Input.LastName;
        employee.Email = Input.Email;
        employee.Phone = Input.Phone;
        employee.DateOfBirth = Input.DateOfBirth;
        employee.HireDate = Input.HireDate;
        employee.DepartmentId = Input.DepartmentId;
        employee.ManagerId = Input.ManagerId;
        employee.JobTitle = Input.JobTitle;
        employee.EmploymentType = Input.EmploymentType;
        employee.Salary = Input.Salary;

        await _employeeService.UpdateAsync(employee);

        TempData["SuccessMessage"] = $"Employee {employee.FullName} updated successfully.";
        return RedirectToPage("Details", new { id = employee.Id });
    }

    private async Task PopulateDropdowns()
    {
        var departments = await _departmentService.GetAllFlatAsync();
        DepartmentList = new SelectList(departments, "Id", "Name");

        var managers = await _employeeService.GetAllActiveAsync();
        ManagerList = new SelectList(managers, "Id", "FullName");
    }
}
