using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController(IMemberService service) : ControllerBase
{
    /// <summary>List members with search, filter, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<MemberListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get member details including active membership info.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MemberDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var member = await service.GetByIdAsync(id, ct);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Register a new member.</summary>
    [HttpPost]
    [ProducesResponseType<MemberDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto, CancellationToken ct)
    {
        var member = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update member profile.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<MemberDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto, CancellationToken ct)
    {
        var member = await service.UpdateAsync(id, dto, ct);
        return Ok(member);
    }

    /// <summary>Deactivate a member. Fails if they have future bookings.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Get member's bookings with optional filters and pagination.</summary>
    [HttpGet("{id:int}/bookings")]
    [ProducesResponseType<PaginatedResponse<BookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookings(
        int id,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get member's upcoming confirmed bookings.</summary>
    [HttpGet("{id:int}/bookings/upcoming")]
    [ProducesResponseType<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUpcomingBookings(int id, CancellationToken ct)
    {
        var result = await service.GetUpcomingBookingsAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Get membership history for a member.</summary>
    [HttpGet("{id:int}/memberships")]
    [ProducesResponseType<IReadOnlyList<MembershipDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberships(int id, CancellationToken ct)
    {
        var result = await service.GetMemberMembershipsAsync(id, ct);
        return Ok(result);
    }
}
