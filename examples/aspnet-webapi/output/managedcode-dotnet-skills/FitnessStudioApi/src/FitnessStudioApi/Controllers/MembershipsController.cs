using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController(IMembershipService service) : ControllerBase
{
    /// <summary>Purchase/create a membership for a member.</summary>
    [HttpPost]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto, CancellationToken ct)
    {
        var membership = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    /// <summary>Get membership details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var membership = await service.GetByIdAsync(id, ct);
        return membership is null ? NotFound() : Ok(membership);
    }

    /// <summary>Cancel a membership.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var membership = await service.CancelAsync(id, ct);
        return Ok(membership);
    }

    /// <summary>Freeze a membership (7-30 days).</summary>
    [HttpPost("{id:int}/freeze")]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto, CancellationToken ct)
    {
        var membership = await service.FreezeAsync(id, dto, ct);
        return Ok(membership);
    }

    /// <summary>Unfreeze a membership. EndDate is extended by freeze duration.</summary>
    [HttpPost("{id:int}/unfreeze")]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken ct)
    {
        var membership = await service.UnfreezeAsync(id, ct);
        return Ok(membership);
    }

    /// <summary>Renew an expired or cancelled membership.</summary>
    [HttpPost("{id:int}/renew")]
    [ProducesResponseType<MembershipDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Renew(int id, CancellationToken ct)
    {
        var membership = await service.RenewAsync(id, ct);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }
}
