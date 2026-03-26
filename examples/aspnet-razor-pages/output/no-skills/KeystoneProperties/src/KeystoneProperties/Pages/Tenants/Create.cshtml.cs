using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services.Interfaces;

namespace KeystoneProperties.Pages.Tenants;

public class CreateModel : PageModel
{
    private readonly ITenantService _tenantService;
    public CreateModel(ITenantService tenantService) => _tenantService = tenantService;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100), Display(Name = "First Name")] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100), Display(Name = "Last Name")] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
        [Required, Phone] public string Phone { get; set; } = string.Empty;
        [Required, Display(Name = "Date of Birth")] public DateOnly DateOfBirth { get; set; }
        [Required, MaxLength(200), Display(Name = "Emergency Contact Name")] public string EmergencyContactName { get; set; } = string.Empty;
        [Required, Phone, Display(Name = "Emergency Contact Phone")] public string EmergencyContactPhone { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Validate age 18+
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - Input.DateOfBirth.Year;
        if (Input.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 18)
        {
            ModelState.AddModelError("Input.DateOfBirth", "Tenant must be at least 18 years old.");
            return Page();
        }

        var tenant = new Tenant
        {
            FirstName = Input.FirstName, LastName = Input.LastName, Email = Input.Email,
            Phone = Input.Phone, DateOfBirth = Input.DateOfBirth,
            EmergencyContactName = Input.EmergencyContactName, EmergencyContactPhone = Input.EmergencyContactPhone
        };

        await _tenantService.CreateAsync(tenant);
        TempData["SuccessMessage"] = $"Tenant '{tenant.FullName}' added successfully.";
        return RedirectToPage("Details", new { id = tenant.Id });
    }
}
