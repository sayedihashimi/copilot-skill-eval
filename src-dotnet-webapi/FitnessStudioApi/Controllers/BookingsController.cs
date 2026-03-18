using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Book a class")]
    [EndpointDescription("Creates a new booking for a member. Enforces all business rules: membership required, premium access, weekly limits, booking window, no overlaps, and capacity/waitlist management.")]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request, CancellationToken ct)
    {
        var booking = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get booking details")]
    [EndpointDescription("Returns full details of a specific booking including class and member info.")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken ct)
    {
        var booking = await service.GetByIdAsync(id, ct);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel a booking")]
    [EndpointDescription("Cancels a booking. Late cancellations (< 2 hours before class) are marked. Cancelled confirmed bookings auto-promote the first waitlisted member.")]
    public async Task<ActionResult<BookingResponse>> Cancel(
        int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await service.CancelAsync(id, request, ct);
        return Ok(booking);
    }

    [HttpPost("{id}/check-in")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Check in to a class")]
    [EndpointDescription("Checks in a member for their booking. Valid from 15 minutes before to 15 minutes after class start.")]
    public async Task<ActionResult<BookingResponse>> CheckIn(int id, CancellationToken ct)
    {
        var booking = await service.CheckInAsync(id, ct);
        return Ok(booking);
    }

    [HttpPost("{id}/no-show")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Mark booking as no-show")]
    [EndpointDescription("Marks a confirmed booking as no-show. Only possible after 15 minutes past class start time.")]
    public async Task<ActionResult<BookingResponse>> MarkNoShow(int id, CancellationToken ct)
    {
        var booking = await service.MarkNoShowAsync(id, ct);
        return Ok(booking);
    }
}
