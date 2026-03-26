using FitnessStudioApi.DTOs.Booking;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    /// <summary>Book a class (enforces all booking rules)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>Get booking details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _service.GetByIdAsync(id);
        return Ok(booking);
    }

    /// <summary>Cancel a booking (enforces cancellation policy)</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingDto dto)
    {
        var booking = await _service.CancelAsync(id, dto);
        return Ok(booking);
    }

    /// <summary>Check in for a class</summary>
    [HttpPost("{id}/check-in")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(int id)
    {
        var booking = await _service.CheckInAsync(id);
        return Ok(booking);
    }

    /// <summary>Mark a booking as no-show</summary>
    [HttpPost("{id}/no-show")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NoShow(int id)
    {
        var booking = await _service.MarkNoShowAsync(id);
        return Ok(booking);
    }
}
