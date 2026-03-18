using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/memberships")]
public class MembershipsController(IMembershipService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new membership")]
    [EndpointDescription("Creates a new membership for a member with a specified plan. Member cannot have an existing active or frozen membership.")]
    public async Task<ActionResult<MembershipResponse>> Create(
        CreateMembershipRequest request, CancellationToken ct)
    {
        var membership = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get membership details")]
    [EndpointDescription("Returns full details of a specific membership including plan and member info.")]
    public async Task<ActionResult<MembershipResponse>> GetById(int id, CancellationToken ct)
    {
        var membership = await service.GetByIdAsync(id, ct);
        return membership is null ? NotFound() : Ok(membership);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel a membership")]
    [EndpointDescription("Cancels an active or frozen membership.")]
    public async Task<ActionResult<MembershipResponse>> Cancel(int id, CancellationToken ct)
    {
        var membership = await service.CancelAsync(id, ct);
        return Ok(membership);
    }

    [HttpPost("{id}/freeze")]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Freeze a membership")]
    [EndpointDescription("Freezes an active membership for 7-30 days. Only allowed once per membership term.")]
    public async Task<ActionResult<MembershipResponse>> Freeze(
        int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await service.FreezeAsync(id, request, ct);
        return Ok(membership);
    }

    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Unfreeze a membership")]
    [EndpointDescription("Unfreezes a frozen membership and extends the end date by the frozen duration.")]
    public async Task<ActionResult<MembershipResponse>> Unfreeze(int id, CancellationToken ct)
    {
        var membership = await service.UnfreezeAsync(id, ct);
        return Ok(membership);
    }

    [HttpPost("{id}/renew")]
    [ProducesResponseType<MembershipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Renew an expired membership")]
    [EndpointDescription("Renews an expired membership starting from today. Member cannot have another active membership.")]
    public async Task<ActionResult<MembershipResponse>> Renew(int id, CancellationToken ct)
    {
        var membership = await service.RenewAsync(id, ct);
        return Ok(membership);
    }
}
