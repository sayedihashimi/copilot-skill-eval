using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.Booking;
using FitnessStudioApi.DTOs.Member;
using FitnessStudioApi.DTOs.Membership;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public sealed class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service)
    {
        _service = service;
    }

    /// <summary>List members with search and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<MemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, isActive, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get member details including active membership and booking stats</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return Ok(member);
    }

    /// <summary>Register a new member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update a member's profile</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _service.UpdateAsync(id, dto);
        return Ok(member);
    }

    /// <summary>Deactivate a member</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get a member's bookings with filters</summary>
    [HttpGet("{id}/bookings")]
    [ProducesResponseType(typeof(PaginatedResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookings(
        int id,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetBookingsAsync(id, status, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a member's upcoming confirmed bookings</summary>
    [HttpGet("{id}/bookings/upcoming")]
    [ProducesResponseType(typeof(List<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
    {
        var bookings = await _service.GetUpcomingBookingsAsync(id);
        return Ok(bookings);
    }

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id}/memberships")]
    [ProducesResponseType(typeof(List<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberships(int id)
    {
        var memberships = await _service.GetMembershipsAsync(id);
        return Ok(memberships);
    }
}
