using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController(IPatronService patronService) : ControllerBase
{
    /// <summary>List patrons with search by name/email, filter by membership type, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatronResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatrons(
        [FromQuery] string? search,
        [FromQuery] string? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronsAsync(search, membershipType, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron details with summary (active loans count, total unpaid fines).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatronDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatron(int id)
    {
        var patron = await patronService.GetPatronByIdAsync(id);
        return patron is null ? NotFound() : Ok(patron);
    }

    /// <summary>Create a new patron.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatron([FromBody] CreatePatronRequest request)
    {
        var patron = await patronService.CreatePatronAsync(request);
        return CreatedAtAction(nameof(GetPatron), new { id = patron.Id }, patron);
    }

    /// <summary>Update an existing patron.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatronResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatron(int id, [FromBody] UpdatePatronRequest request)
    {
        var patron = await patronService.UpdatePatronAsync(id, request);
        return patron is null ? NotFound() : Ok(patron);
    }

    /// <summary>Deactivate patron (set IsActive = false; fails if patron has active loans).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivatePatron(int id)
    {
        await patronService.DeactivatePatronAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans (filter by status: active, returned, overdue).</summary>
    [HttpGet("{id}/loans")]
    [ProducesResponseType(typeof(PagedResult<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronLoansAsync(id, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's reservations.</summary>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronReservationsAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's fines (filter by status: unpaid, paid, waived).</summary>
    [HttpGet("{id}/fines")]
    [ProducesResponseType(typeof(PagedResult<FineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronFines(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronFinesAsync(id, status, page, pageSize);
        return Ok(result);
    }
}
