using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController(IMemberService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<MemberListResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List members")]
    [EndpointDescription("Returns a paginated list of members. Supports search by name/email and filtering by active status.")]
    public async Task<ActionResult<PagedResponse<MemberListResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get member details")]
    [EndpointDescription("Returns full details of a member including active membership and booking stats.")]
    public async Task<ActionResult<MemberResponse>> GetById(int id, CancellationToken ct)
    {
        var member = await service.GetByIdAsync(id, ct);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Register a new member")]
    [EndpointDescription("Creates a new member account. Member must be at least 16 years old and email must be unique.")]
    public async Task<ActionResult<MemberResponse>> Create(
        CreateMemberRequest request, CancellationToken ct)
    {
        var member = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<MemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update member profile")]
    [EndpointDescription("Updates an existing member's profile information.")]
    public async Task<ActionResult<MemberResponse>> Update(
        int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await service.UpdateAsync(id, request, ct);
        return Ok(member);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Deactivate a member")]
    [EndpointDescription("Deactivates a member. Fails if the member has future confirmed bookings.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeactivateAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id}/bookings")]
    [ProducesResponseType<PagedResponse<BookingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get member bookings")]
    [EndpointDescription("Returns paginated bookings for a member with optional status and date range filters.")]
    public async Task<ActionResult<PagedResponse<BookingResponse>>> GetBookings(
        int id,
        [FromQuery] BookingStatus? status,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id}/bookings/upcoming")]
    [ProducesResponseType<List<BookingResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get upcoming bookings")]
    [EndpointDescription("Returns all upcoming confirmed bookings for a member sorted by class start time.")]
    public async Task<ActionResult<List<BookingResponse>>> GetUpcomingBookings(int id, CancellationToken ct)
    {
        var result = await service.GetUpcomingBookingsAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("{id}/memberships")]
    [ProducesResponseType<List<MembershipResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get membership history")]
    [EndpointDescription("Returns all memberships for a member, ordered by start date descending.")]
    public async Task<ActionResult<List<MembershipResponse>>> GetMemberships(int id, CancellationToken ct)
    {
        var result = await service.GetMembershipHistoryAsync(id, ct);
        return Ok(result);
    }
}
