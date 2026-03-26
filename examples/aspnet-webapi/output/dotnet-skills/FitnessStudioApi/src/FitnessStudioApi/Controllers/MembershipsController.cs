using FitnessStudioApi.DTOs.Membership;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public sealed class MembershipsController : ControllerBase
{
    private readonly IMembershipService _service;

    public MembershipsController(IMembershipService service)
    {
        _service = service;
    }

    /// <summary>Purchase/create a membership for a member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        var membership = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var membership = await _service.GetByIdAsync(id);
        return Ok(membership);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        var membership = await _service.CancelAsync(id);
        return Ok(membership);
    }

    /// <summary>Freeze a membership</summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto)
    {
        var membership = await _service.FreezeAsync(id, dto);
        return Ok(membership);
    }

    /// <summary>Unfreeze a membership</summary>
    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var membership = await _service.UnfreezeAsync(id);
        return Ok(membership);
    }

    /// <summary>Renew an expired membership</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Renew(int id)
    {
        var membership = await _service.RenewAsync(id);
        return Ok(membership);
    }
}
