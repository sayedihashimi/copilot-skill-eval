using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatronsController(IPatronService patronService, IValidator<CreatePatronRequest> createValidator, IValidator<UpdatePatronRequest> updateValidator) : ControllerBase
{
    /// <summary>List patrons with search by name/email, filter by membership type, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PatronResponse>), StatusCodes.Status200OK)]
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
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PatronResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatron(int id)
    {
        var result = await patronService.GetPatronByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new patron.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatronResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatron([FromBody] CreatePatronRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await patronService.CreatePatronAsync(request);
        return CreatedAtAction(nameof(GetPatron), new { id = result.Id }, result);
    }

    /// <summary>Update an existing patron.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PatronResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePatron(int id, [FromBody] UpdatePatronRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await patronService.UpdatePatronAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Deactivate patron (set IsActive = false; fails if patron has active loans).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivatePatron(int id)
    {
        await patronService.DeactivatePatronAsync(id);
        return NoContent();
    }

    /// <summary>Get patron's loans (filter by status: active, returned, overdue).</summary>
    [HttpGet("{id:int}/loans")]
    [ProducesResponseType(typeof(PaginatedResponse<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronLoans(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronLoansAsync(id, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's reservations.</summary>
    [HttpGet("{id:int}/reservations")]
    [ProducesResponseType(typeof(PaginatedResponse<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronReservations(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronReservationsAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get patron's fines (filter by status: unpaid, paid, waived).</summary>
    [HttpGet("{id:int}/fines")]
    [ProducesResponseType(typeof(PaginatedResponse<FineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatronFines(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await patronService.GetPatronFinesAsync(id, status, page, pageSize);
        return Ok(result);
    }
}
