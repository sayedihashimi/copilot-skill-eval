using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController : ControllerBase
{
    private readonly IPatronService _service;

    public PatronsController(IPatronService service) => _service = service;

    /// <summary>List patrons with search and filter by membership type</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatronDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? membershipType, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, membershipType, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron details with active loans count and unpaid fines balance</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new patron</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronDto), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] PatronCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing patron</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] PatronUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Deactivate a patron (fails if patron has active loans)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans with optional status filter</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPatronLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPatronLoansAsync(id, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's reservations</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPatronReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPatronReservationsAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's fines with optional status filter</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PagedResult<FineDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPatronFines(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPatronFinesAsync(id, status, page, pageSize);
        return Ok(result);
    }
}
