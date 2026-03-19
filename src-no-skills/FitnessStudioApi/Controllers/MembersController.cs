using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service)
    {
        _service = service;
    }

    /// <summary>List members with search, filter, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MemberDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetAllAsync(search, isActive, pagination);
        return Ok(result);
    }

    /// <summary>Get member details with active membership and booking stats</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return member == null ? NotFound() : Ok(member);
    }

    /// <summary>Register a new member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        var member = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    /// <summary>Update member profile</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _service.UpdateAsync(id, dto);
        return member == null ? NotFound() : Ok(member);
    }

    /// <summary>Deactivate member (fails if they have future bookings)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>Get member's bookings with filters and pagination</summary>
    [HttpGet("{id}/bookings")]
    [ProducesResponseType(typeof(PagedResult<BookingDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBookings(int id, [FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetMemberBookingsAsync(id, status, fromDate, toDate, pagination);
        return Ok(result);
    }

    /// <summary>Get member's upcoming confirmed bookings</summary>
    [HttpGet("{id}/bookings/upcoming")]
    [ProducesResponseType(typeof(List<BookingDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
    {
        var result = await _service.GetUpcomingBookingsAsync(id);
        return Ok(result);
    }

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id}/memberships")]
    [ProducesResponseType(typeof(List<MembershipDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMemberships(int id)
    {
        var result = await _service.GetMemberMembershipsAsync(id);
        return Ok(result);
    }
}
