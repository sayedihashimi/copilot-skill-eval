using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BookingResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<BookingResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, CancelBookingRequest request)
        => Ok(await service.CancelAsync(id, request));

    [HttpPost("{id:int}/check-in")]
    [ProducesResponseType<BookingResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckIn(int id)
        => Ok(await service.CheckInAsync(id));

    [HttpPost("{id:int}/no-show")]
    [ProducesResponseType<BookingResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> NoShow(int id)
        => Ok(await service.MarkNoShowAsync(id));
}
