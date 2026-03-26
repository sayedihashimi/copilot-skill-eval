using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController(IBookingService service) : ControllerBase
{
    /// <summary>Book a class. Enforces all booking rules.</summary>
    [HttpPost]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
    {
        var booking = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>Get booking details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var booking = await service.GetByIdAsync(id, ct);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancel a booking. Promotes from waitlist if applicable.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingDto dto, CancellationToken ct)
    {
        var booking = await service.CancelAsync(id, dto, ct);
        return Ok(booking);
    }

    /// <summary>Check in for a class. Available 15 min before to 15 min after start.</summary>
    [HttpPost("{id:int}/check-in")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn(int id, CancellationToken ct)
    {
        var booking = await service.CheckInAsync(id, ct);
        return Ok(booking);
    }

    /// <summary>Mark a booking as no-show.</summary>
    [HttpPost("{id:int}/no-show")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> NoShow(int id, CancellationToken ct)
    {
        var booking = await service.MarkNoShowAsync(id, ct);
        return Ok(booking);
    }
}
