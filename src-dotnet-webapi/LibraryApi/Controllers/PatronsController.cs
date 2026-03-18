using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatronsController(IPatronService patronService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<PatronResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List patrons")]
    [EndpointDescription("Returns a paginated list of patrons with optional name/email search and membership type filter.")]
    public async Task<ActionResult<PagedResponse<PatronResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] MembershipType? membershipType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await patronService.GetAllAsync(search, membershipType, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PatronDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get patron by ID")]
    [EndpointDescription("Returns patron details including active loan count and unpaid fines balance.")]
    public async Task<ActionResult<PatronDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var patron = await patronService.GetByIdAsync(id, cancellationToken);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpPost]
    [ProducesResponseType<PatronResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a patron")]
    [EndpointDescription("Creates a new library patron. Email must be unique.")]
    public async Task<ActionResult<PatronResponse>> Create(CreatePatronRequest request, CancellationToken cancellationToken)
    {
        var patron = await patronService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patron.Id }, patron);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<PatronResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a patron")]
    [EndpointDescription("Updates an existing patron's information.")]
    public async Task<ActionResult<PatronResponse>> Update(int id, UpdatePatronRequest request, CancellationToken cancellationToken)
    {
        var patron = await patronService.UpdateAsync(id, request, cancellationToken);
        return patron is null ? NotFound() : Ok(patron);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Deactivate a patron")]
    [EndpointDescription("Deactivates a patron. Fails if the patron has active loans.")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await patronService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/loans")]
    [ProducesResponseType<PagedResponse<LoanResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get patron loans")]
    [EndpointDescription("Returns loans for a specific patron, optionally filtered by status.")]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetPatronLoans(
        int id,
        [FromQuery] LoanStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await patronService.GetPatronLoansAsync(id, status, page, pageSize, cancellationToken));
    }

    [HttpGet("{id}/reservations")]
    [ProducesResponseType<List<ReservationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get patron reservations")]
    [EndpointDescription("Returns active reservations for a specific patron.")]
    public async Task<ActionResult<List<ReservationResponse>>> GetPatronReservations(int id, CancellationToken cancellationToken)
    {
        return Ok(await patronService.GetPatronReservationsAsync(id, cancellationToken));
    }

    [HttpGet("{id}/fines")]
    [ProducesResponseType<List<FineResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get patron fines")]
    [EndpointDescription("Returns fines for a specific patron, optionally filtered by status.")]
    public async Task<ActionResult<List<FineResponse>>> GetPatronFines(
        int id,
        [FromQuery] FineStatus? status,
        CancellationToken cancellationToken = default)
    {
        return Ok(await patronService.GetPatronFinesAsync(id, status, cancellationToken));
    }
}
