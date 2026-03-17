using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
public sealed class MembersController(IMemberService service) : ControllerBase
{
    private readonly IMemberService _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MemberListResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(search, isActive, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberResponse>> GetById(int id, CancellationToken ct)
    {
        var member = await _service.GetByIdAsync(id, ct);
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(CreateMemberRequest request, CancellationToken ct)
    {
        var member = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberResponse>> Update(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await _service.UpdateAsync(id, request, ct);
        return Ok(member);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/bookings")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetBookings(
        int id,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetBookingsAsync(id, status, from, to, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/bookings/upcoming")]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetUpcomingBookings(int id, CancellationToken ct)
    {
        var bookings = await _service.GetUpcomingBookingsAsync(id, ct);
        return Ok(bookings);
    }

    [HttpGet("{id:int}/memberships")]
    public async Task<ActionResult<IReadOnlyList<MembershipResponse>>> GetMemberships(int id, CancellationToken ct)
    {
        var memberships = await _service.GetMembershipsAsync(id, ct);
        return Ok(memberships);
    }
}
