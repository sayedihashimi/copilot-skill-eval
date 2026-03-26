using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class TicketTypesModel : PageModel
{
    private readonly IEventService _eventService;

    public TicketTypesModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event? Event { get; set; }
    public List<TicketType> TicketTypes { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Early-Bird Price")]
        public decimal? EarlyBirdPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetByIdAsync(eventId);
        if (Event == null) return NotFound();

        TicketTypes = await _eventService.GetTicketTypesAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int eventId)
    {
        Event = await _eventService.GetByIdAsync(eventId);
        if (Event == null) return NotFound();

        if (!ModelState.IsValid)
        {
            TicketTypes = await _eventService.GetTicketTypesAsync(eventId);
            return Page();
        }

        var ticketType = new TicketType
        {
            EventId = eventId,
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            EarlyBirdPrice = Input.EarlyBirdPrice,
            Quantity = Input.Quantity,
            SortOrder = Input.SortOrder,
            IsActive = true
        };

        await _eventService.CreateTicketTypeAsync(ticketType);
        TempData["SuccessMessage"] = "Ticket type added.";
        return RedirectToPage("TicketTypes", new { eventId });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int ticketTypeId, int eventId)
    {
        var tt = await _eventService.GetTicketTypeByIdAsync(ticketTypeId);
        if (tt == null) return NotFound();

        tt.IsActive = !tt.IsActive;
        await _eventService.UpdateTicketTypeAsync(tt);

        TempData["SuccessMessage"] = $"Ticket type '{tt.Name}' {(tt.IsActive ? "activated" : "deactivated")}.";
        return RedirectToPage("TicketTypes", new { eventId });
    }
}
