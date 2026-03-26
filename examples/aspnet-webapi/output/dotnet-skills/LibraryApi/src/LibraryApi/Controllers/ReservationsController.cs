using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    /// <summary>List reservations with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await reservationService.GetReservationsAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get reservation details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(int id)
    {
        var reservation = await reservationService.GetReservationByIdAsync(id);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    /// <summary>Create a reservation enforcing all reservation rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var reservation = await reservationService.CreateReservationAsync(request);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var reservation = await reservationService.CancelReservationAsync(id);
        return Ok(reservation);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron).</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FulfillReservation(int id)
    {
        var loan = await reservationService.FulfillReservationAsync(id);
        return Ok(loan);
    }
}
