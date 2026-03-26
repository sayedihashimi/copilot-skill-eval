using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Employees;

public class CreateModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public CreateModel(IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList DepartmentList { get; set; } = default!;
    public SelectList ManagerList { get; set; } = default!;

    public class InputModel
    {
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
        public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

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

    public async Task<IActionResult> OnGetAsync()
    {
        await PopulateDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return Page();
        }

        var employee = new Employee
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            DateOfBirth = Input.DateOfBirth,
            HireDate = Input.HireDate,
            DepartmentId = Input.DepartmentId,
            ManagerId = Input.ManagerId,
            JobTitle = Input.JobTitle,
            EmploymentType = Input.EmploymentType,
            Salary = Input.Salary
        };

        var created = await _employeeService.CreateAsync(employee);

        TempData["SuccessMessage"] = $"Employee {created.FullName} ({created.EmployeeNumber}) created successfully.";
        return RedirectToPage("Details", new { id = created.Id });
    }

    private async Task PopulateDropdowns()
    {
        var departments = await _departmentService.GetAllFlatAsync();
        DepartmentList = new SelectList(departments, "Id", "Name");

        var managers = await _employeeService.GetAllActiveAsync();
        ManagerList = new SelectList(managers, "Id", "FullName");
    }
}
