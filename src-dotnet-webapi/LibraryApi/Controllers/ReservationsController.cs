using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ReservationResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List reservations")]
    [EndpointDescription("Returns a paginated list of reservations, optionally filtered by status.")]
    public async Task<ActionResult<PagedResponse<ReservationResponse>>> GetAll(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await reservationService.GetAllAsync(status, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get reservation by ID")]
    [EndpointDescription("Returns details of a specific reservation.")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.GetByIdAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a reservation")]
    [EndpointDescription("Creates a new reservation. Patron must be active and not already have the book on loan or reservation.")]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await reservationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel a reservation")]
    [EndpointDescription("Cancels a pending or ready reservation and adjusts the queue.")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await reservationService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/fulfill")]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Fulfill a reservation")]
    [EndpointDescription("Fulfills a ready reservation by creating a loan for the patron.")]
    public async Task<ActionResult<LoanResponse>> Fulfill(int id, CancellationToken cancellationToken)
    {
        return Ok(await reservationService.FulfillAsync(id, cancellationToken));
    }
}
