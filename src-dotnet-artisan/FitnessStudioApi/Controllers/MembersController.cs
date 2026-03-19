using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController(IMemberService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<MemberResponse>>(200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] PaginationParams pagination)
        => Ok(await service.GetAllAsync(search, isActive, pagination));

    [HttpGet("{id:int}")]
    [ProducesResponseType<MemberDetailResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<MemberResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateMemberRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<MemberResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateMemberRequest request)
        => Ok(await service.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/bookings")]
    [ProducesResponseType<PaginatedResponse<BookingResponse>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBookings(int id, [FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] PaginationParams pagination)
        => Ok(await service.GetBookingsAsync(id, status, from, to, pagination));

    [HttpGet("{id:int}/bookings/upcoming")]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUpcomingBookings(int id)
        => Ok(await service.GetUpcomingBookingsAsync(id));

    [HttpGet("{id:int}/memberships")]
    [ProducesResponseType<IReadOnlyList<MembershipResponse>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMemberships(int id)
        => Ok(await service.GetMembershipsAsync(id));
}
