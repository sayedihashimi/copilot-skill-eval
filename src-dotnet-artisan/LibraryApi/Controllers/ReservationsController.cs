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
    public async Task<ActionResult<PagedResult<ReservationResponse>>> GetAll(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await reservationService.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id)
    {
        var reservation = await reservationService.GetByIdAsync(id);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request)
    {
        var (reservation, error) = await reservationService.CreateAsync(request);
        if (reservation is null)
            return BadRequest(new ProblemDetails { Title = "Reservation failed", Detail = error, Status = 400 });
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(int id)
    {
        var (reservation, error) = await reservationService.CancelAsync(id);
        if (reservation is null)
            return BadRequest(new ProblemDetails { Title = "Cancellation failed", Detail = error, Status = 400 });
        return Ok(reservation);
    }

    [HttpPost("{id:int}/fulfill")]
    public async Task<ActionResult<LoanResponse>> Fulfill(int id)
    {
        var (loan, error) = await reservationService.FulfillAsync(id);
        if (loan is null)
            return BadRequest(new ProblemDetails { Title = "Fulfillment failed", Detail = error, Status = 400 });
        return Ok(loan);
    }
}
