using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ReservationsController(IReservationService reservationService) : ControllerBase
{
    /// <summary>List reservations with filter by status and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations([FromQuery] ReservationStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await reservationService.GetReservationsAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get reservation details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(int id)
    {
        var reservation = await reservationService.GetReservationByIdAsync(id);
        return reservation is not null ? Ok(reservation) : NotFound();
    }

    /// <summary>Create a reservation enforcing all reservation rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var reservation = await reservationService.CreateReservationAsync(request);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var reservation = await reservationService.CancelReservationAsync(id);
        return Ok(reservation);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron).</summary>
    [HttpPost("{id:int}/fulfill")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FulfillReservation(int id)
    {
        var loan = await reservationService.FulfillReservationAsync(id);
        return Ok(loan);
    }
}
