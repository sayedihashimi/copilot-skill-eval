using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CheckInProcessModel : PageModel
{
    private readonly ICheckInService _checkInService;
    private readonly IRegistrationService _registrationService;

    public CheckInProcessModel(ICheckInService checkInService, IRegistrationService registrationService)
    {
        _checkInService = checkInService;
        _registrationService = registrationService;
    }

    public Registration? Registration { get; set; }
    public int EventId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int RegistrationId { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Staff Name")]
        public string CheckedInBy { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId, int registrationId)
    {
        EventId = eventId;
        Registration = await _registrationService.GetByIdAsync(registrationId);
        if (Registration == null || Registration.EventId != eventId) return NotFound();

        Input.RegistrationId = registrationId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        EventId = eventId;
        Registration = await _registrationService.GetByIdAsync(Input.RegistrationId);
        if (Registration == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        var (checkIn, error) = await _checkInService.ProcessCheckInAsync(Input.RegistrationId, Input.CheckedInBy, Input.Notes);
        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return Page();
        }

        TempData["SuccessMessage"] = $"Check-in complete for {Registration.Attendee.FullName}.";
        return RedirectToPage("CheckIn", new { eventId });
    }
}
