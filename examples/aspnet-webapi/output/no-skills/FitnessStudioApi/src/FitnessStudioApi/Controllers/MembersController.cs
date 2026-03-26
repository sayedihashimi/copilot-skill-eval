using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service) => _service = service;

    /// <summary>List members with search, filter, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<MemberListDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
        => Ok(await _service.GetAllAsync(page, pageSize, search, isActive));

    /// <summary>Get member details with active membership</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberDto), 200)]
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
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a member</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Get member's bookings with filters</summary>
    [HttpGet("{id}/bookings")]
    [ProducesResponseType(typeof(PaginatedResult<BookingDto>), 200)]
    public async Task<IActionResult> GetBookings(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        => Ok(await _service.GetBookingsAsync(id, page, pageSize, status, fromDate, toDate));

    /// <summary>Get member's upcoming confirmed bookings</summary>
    [HttpGet("{id}/bookings/upcoming")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), 200)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
        => Ok(await _service.GetUpcomingBookingsAsync(id));

    /// <summary>Get membership history for a member</summary>
    [HttpGet("{id}/memberships")]
    [ProducesResponseType(typeof(IEnumerable<MembershipDto>), 200)]
    public async Task<IActionResult> GetMemberships(int id)
        => Ok(await _service.GetMembershipsAsync(id));
}
