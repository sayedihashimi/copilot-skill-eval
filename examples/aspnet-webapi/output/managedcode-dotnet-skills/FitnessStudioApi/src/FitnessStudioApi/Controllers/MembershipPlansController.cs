using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController(IMembershipPlanService service) : ControllerBase
{
    /// <summary>List all active membership plans.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MembershipPlanDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var plans = await service.GetAllActiveAsync(ct);
        return Ok(plans);
    }

    /// <summary>Get membership plan details by Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipPlanDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var plan = await service.GetByIdAsync(id, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan.</summary>
    [HttpPost]
    [ProducesResponseType<MembershipPlanDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipPlanDto dto, CancellationToken ct)
    {
        var plan = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update an existing membership plan.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<MembershipPlanDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto, CancellationToken ct)
    {
        var plan = await service.UpdateAsync(id, dto, ct);
        return Ok(plan);
    }

    /// <summary>Deactivate a membership plan.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
