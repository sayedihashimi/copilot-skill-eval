using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatronsController(IPatronService patronService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PatronDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await patronService.GetAllAsync(search, membershipType, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatronDetailDto>> GetById(int id, CancellationToken ct = default)
    {
        var patron = await patronService.GetByIdAsync(id, ct);
        return patron is not null ? Ok(patron) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PatronDto>> Create([FromBody] CreatePatronRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "FirstName and LastName are required." });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Email is required." });

        var patron = await patronService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatronDto>> Update(int id, [FromBody] UpdatePatronRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "FirstName and LastName are required." });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new ProblemDetails { Title = "Validation Error", Detail = "Email is required." });

        var patron = await patronService.UpdateAsync(id, request, ct);
        return patron is not null ? Ok(patron) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await patronService.DeactivateAsync(id, ct);
        if (!found) return NotFound();
        if (hasActiveLoans) return Conflict(new ProblemDetails { Title = "Conflict", Detail = "Cannot deactivate patron with active loans." });
        return NoContent();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<PaginatedResponse<LoanDto>>> GetLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await patronService.GetLoansAsync(id, status, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetReservations(int id, CancellationToken ct = default)
    {
        var result = await patronService.GetReservationsAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/fines")]
    public async Task<ActionResult<IReadOnlyList<FineDto>>> GetFines(int id, [FromQuery] string? status, CancellationToken ct = default)
    {
        var result = await patronService.GetFinesAsync(id, status, ct);
        return Ok(result);
    }
}
