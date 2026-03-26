using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RegisterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAttendeeService _attendeeService;
    private readonly IRegistrationService _registrationService;

    public RegisterModel(IEventService eventService, IAttendeeService attendeeService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _attendeeService = attendeeService;
        _registrationService = registrationService;
    }

    public Event? Event { get; set; }
    public List<SelectListItem> AttendeeOptions { get; set; } = new();
    public List<SelectListItem> TicketTypeOptions { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Attendee")]
        public int AttendeeId { get; set; }

        [Required]
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetByIdWithDetailsAsync(eventId);
        if (Event == null) return NotFound();

        await LoadOptionsAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId)
    {
        Event = await _eventService.GetByIdWithDetailsAsync(eventId);
        if (Event == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync(eventId);
            return Page();
        }

        var (registration, error) = await _registrationService.RegisterAsync(
            eventId, Input.AttendeeId, Input.TicketTypeId, Input.SpecialRequests);

        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            await LoadOptionsAsync(eventId);
            return Page();
        }

        TempData["SuccessMessage"] = $"Registration successful! Confirmation number: {registration!.ConfirmationNumber}";
        return RedirectToPage("/Registrations/Details", new { id = registration.Id });
    }

    private async Task LoadOptionsAsync(int eventId)
    {
        var attendees = await _attendeeService.GetAllAsync();
        AttendeeOptions = attendees.Select(a => new SelectListItem($"{a.FullName} ({a.Email})", a.Id.ToString())).ToList();

        var ticketTypes = await _eventService.GetTicketTypesAsync(eventId);
        TicketTypeOptions = ticketTypes
            .Where(t => t.IsActive)
            .Select(t => new SelectListItem(
                $"{t.Name} — {t.Price:C} ({t.Quantity - t.QuantitySold} remaining)",
                t.Id.ToString()))
            .ToList();
    }
}
