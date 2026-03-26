using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _service;

    public MembershipsController(IMembershipService service) => _service = service;

    /// <summary>Purchase/create a membership for a member</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
    {
        var ms = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ms.Id }, ms);
    }

    /// <summary>Get membership details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var ms = await _service.GetByIdAsync(id);
        return ms == null ? NotFound() : Ok(ms);
    }

    /// <summary>Cancel a membership</summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Cancel(int id)
        => Ok(await _service.CancelAsync(id));

    /// <summary>Freeze a membership</summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Freeze(int id, [FromBody] FreezeMembershipDto dto)
        => Ok(await _service.FreezeAsync(id, dto));

    /// <summary>Unfreeze a membership</summary>
    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType(typeof(MembershipDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Unfreeze(int id)
        => Ok(await _service.UnfreezeAsync(id));

    /// <summary>Renew an expired or cancelled membership</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(typeof(MembershipDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Renew(int id)
    {
        var ms = await _service.RenewAsync(id);
        return CreatedAtAction(nameof(GetById), new { id = ms.Id }, ms);
    }
}
