using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

    /// <summary>List reservations with optional status filter and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await _service.GetReservationsAsync(status, page, pageSize));

    /// <summary>Get reservation details.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetReservation(int id)
    {
        var res = await _service.GetReservationByIdAsync(id);
        return res == null ? NotFound() : Ok(res);
    }

    /// <summary>Create a reservation enforcing all reservation rules.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var (reservation, error) = await _service.CreateReservationAsync(dto);
        if (reservation == null) return BadRequest(new ProblemDetails { Title = "Reservation denied", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    /// <summary>Cancel a reservation.</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var (reservation, error) = await _service.CancelReservationAsync(id);
        if (reservation == null && error!.Contains("not found")) return NotFound();
        if (reservation == null) return BadRequest(new ProblemDetails { Title = "Cancellation failed", Detail = error, Status = 400 });
        return Ok(reservation);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron).</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FulfillReservation(int id)
    {
        var (loan, error) = await _service.FulfillReservationAsync(id);
        if (loan == null && error!.Contains("not found")) return NotFound();
        if (loan == null) return BadRequest(new ProblemDetails { Title = "Fulfillment failed", Detail = error, Status = 400 });
        return Ok(loan);
    }
}
