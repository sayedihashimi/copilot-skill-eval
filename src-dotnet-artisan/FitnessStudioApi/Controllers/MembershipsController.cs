using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController(IMembershipService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MembershipResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateMembershipRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<MembershipResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id)
        => Ok(await service.CancelAsync(id));

    [HttpPost("{id:int}/freeze")]
    [ProducesResponseType<MembershipResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Freeze(int id, FreezeMembershipRequest request)
        => Ok(await service.FreezeAsync(id, request));

    [HttpPost("{id:int}/unfreeze")]
    [ProducesResponseType<MembershipResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unfreeze(int id)
        => Ok(await service.UnfreezeAsync(id));

    [HttpPost("{id:int}/renew")]
    [ProducesResponseType<MembershipResponse>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Renew(int id)
    {
        var result = await service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
