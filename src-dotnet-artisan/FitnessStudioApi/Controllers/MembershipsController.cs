using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
public sealed class MembershipsController(IMembershipService service) : ControllerBase
{
    private readonly IMembershipService _service = service;

    [HttpPost]
    public async Task<ActionResult<MembershipResponse>> Create(CreateMembershipRequest request, CancellationToken ct)
    {
        var membership = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipResponse>> GetById(int id, CancellationToken ct)
    {
        var membership = await _service.GetByIdAsync(id, ct);
        return Ok(membership);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<MembershipResponse>> Cancel(int id, CancellationToken ct)
    {
        var membership = await _service.CancelAsync(id, ct);
        return Ok(membership);
    }

    [HttpPost("{id:int}/freeze")]
    public async Task<ActionResult<MembershipResponse>> Freeze(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await _service.FreezeAsync(id, request, ct);
        return Ok(membership);
    }

    [HttpPost("{id:int}/unfreeze")]
    public async Task<ActionResult<MembershipResponse>> Unfreeze(int id, CancellationToken ct)
    {
        var membership = await _service.UnfreezeAsync(id, ct);
        return Ok(membership);
    }

    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<MembershipResponse>> Renew(int id, CancellationToken ct)
    {
        var membership = await _service.RenewAsync(id, ct);
        return Ok(membership);
    }
}
