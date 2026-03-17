using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReservationsController(IReservationService reservationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ReservationDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await reservationService.GetAllAsync(status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id, CancellationToken ct = default)
    {
        var reservation = await reservationService.GetByIdAsync(id, ct);
        return reservation is not null ? Ok(reservation) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationRequest request, CancellationToken ct = default)
    {
        var (reservation, error) = await reservationService.CreateAsync(request, ct);
        if (error is not null)
            return BadRequest(new ProblemDetails { Title = "Reservation Failed", Detail = error });
        return CreatedAtAction(nameof(GetById), new { id = reservation!.Id }, reservation);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(int id, CancellationToken ct = default)
    {
        var (reservation, error) = await reservationService.CancelAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Cancellation Failed", Detail = error });
        }
        return Ok(reservation);
    }

    [HttpPost("{id:int}/fulfill")]
    public async Task<ActionResult<LoanDto>> Fulfill(int id, CancellationToken ct = default)
    {
        var (loan, error) = await reservationService.FulfillAsync(id, ct);
        if (error is not null)
        {
            if (error.Contains("not found")) return NotFound();
            return BadRequest(new ProblemDetails { Title = "Fulfillment Failed", Detail = error });
        }
        return Ok(loan);
    }
}
