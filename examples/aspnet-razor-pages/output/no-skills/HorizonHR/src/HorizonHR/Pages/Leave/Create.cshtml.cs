using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Services;

namespace HorizonHR.Pages.Leave;

public class CreateModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList EmployeeOptions { get; set; } = null!;
    public SelectList LeaveTypeOptions { get; set; } = null!;

    public class InputModel
    {
        [Required(ErrorMessage = "Please select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please select a leave type.")]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Please provide a reason for the leave request.")]
        [MaxLength(1000)]
        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await PopulateDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EndDate < Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be on or after the start date.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        var totalDays = await _leaveService.CalculateBusinessDays(Input.StartDate, Input.EndDate);

        var request = new LeaveRequest
        {
            EmployeeId = Input.EmployeeId,
            LeaveTypeId = Input.LeaveTypeId,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            TotalDays = totalDays,
            Reason = Input.Reason
        };

        try
        {
            await _leaveService.SubmitRequestAsync(request);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await PopulateDropdownsAsync();
            return Page();
        }
    }

    private async Task PopulateDropdownsAsync()
    {
        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = new SelectList(employees, "Id", "FullName", Input.EmployeeId);

        var leaveTypes = await _leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = new SelectList(leaveTypes, "Id", "Name", Input.LeaveTypeId);
    }
}
