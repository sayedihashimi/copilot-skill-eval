using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
public class MembershipPlansController(IMembershipPlanService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<MembershipPlanResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List all active membership plans")]
    [EndpointDescription("Returns all active membership plans offered by Zenith Fitness Studio.")]
    public async Task<ActionResult<List<MembershipPlanResponse>>> GetAll(CancellationToken ct)
    {
        var plans = await service.GetAllActiveAsync(ct);
        return Ok(plans);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<MembershipPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a membership plan by ID")]
    [EndpointDescription("Returns the full details of a specific membership plan.")]
    public async Task<ActionResult<MembershipPlanResponse>> GetById(int id, CancellationToken ct)
    {
        var plan = await service.GetByIdAsync(id, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    [ProducesResponseType<MembershipPlanResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new membership plan")]
    [EndpointDescription("Creates a new membership plan with the provided details.")]
    public async Task<ActionResult<MembershipPlanResponse>> Create(
        CreateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<MembershipPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a membership plan")]
    [EndpointDescription("Updates an existing membership plan with the provided details.")]
    public async Task<ActionResult<MembershipPlanResponse>> Update(
        int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await service.UpdateAsync(id, request, ct);
        return Ok(plan);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Deactivate a membership plan")]
    [EndpointDescription("Soft-deletes a membership plan by setting IsActive to false.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeactivateAsync(id, ct);
        return NoContent();
    }
}
