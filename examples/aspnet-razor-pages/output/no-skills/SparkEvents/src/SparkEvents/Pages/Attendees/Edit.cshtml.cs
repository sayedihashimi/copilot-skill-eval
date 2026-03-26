using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class EditModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public EditModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Organization { get; set; }

        [MaxLength(500)]
        [Display(Name = "Dietary Needs")]
        public string? DietaryNeeds { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await _attendeeService.GetByIdAsync(id);
        if (attendee == null) return NotFound();

        Input = new InputModel
        {
            Id = attendee.Id,
            FirstName = attendee.FirstName,
            LastName = attendee.LastName,
            Email = attendee.Email,
            Phone = attendee.Phone,
            Organization = attendee.Organization,
            DietaryNeeds = attendee.DietaryNeeds
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var attendee = await _attendeeService.GetByIdAsync(Input.Id);
        if (attendee == null) return NotFound();

        // Check for duplicate email (if changed)
        if (attendee.Email.ToLower() != Input.Email.ToLower())
        {
            var existing = await _attendeeService.GetByEmailAsync(Input.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Input.Email", "An attendee with this email already exists.");
                return Page();
            }
        }

        attendee.FirstName = Input.FirstName;
        attendee.LastName = Input.LastName;
        attendee.Email = Input.Email;
        attendee.Phone = Input.Phone;
        attendee.Organization = Input.Organization;
        attendee.DietaryNeeds = Input.DietaryNeeds;

        await _attendeeService.UpdateAsync(attendee);
        TempData["SuccessMessage"] = "Attendee updated successfully.";
        return RedirectToPage("Details", new { id = attendee.Id });
    }
}
