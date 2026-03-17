using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(IBookingService service) : ControllerBase
{
    private readonly IBookingService _service = service;

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        var booking = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken ct)
    {
        var booking = await _service.GetByIdAsync(id, ct);
        return Ok(booking);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await _service.CancelAsync(id, request, ct);
        return Ok(booking);
    }

    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<BookingResponse>> CheckIn(int id, CancellationToken ct)
    {
        var booking = await _service.CheckInAsync(id, ct);
        return Ok(booking);
    }

    [HttpPost("{id:int}/no-show")]
    public async Task<ActionResult<BookingResponse>> NoShow(int id, CancellationToken ct)
    {
        var booking = await _service.MarkNoShowAsync(id, ct);
        return Ok(booking);
    }
}
