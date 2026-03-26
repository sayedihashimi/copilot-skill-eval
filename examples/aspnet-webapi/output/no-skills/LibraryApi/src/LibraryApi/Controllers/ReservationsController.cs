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

    /// <summary>List reservations with filter by status and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get reservation details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a reservation enforcing all reservation rules</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Cancel a reservation</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _service.CancelAsync(id);
        return Ok(result);
    }

    /// <summary>Fulfill a "Ready" reservation (creates a loan for the patron)</summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Fulfill(int id)
    {
        var result = await _service.FulfillAsync(id);
        return Ok(result);
    }
}
