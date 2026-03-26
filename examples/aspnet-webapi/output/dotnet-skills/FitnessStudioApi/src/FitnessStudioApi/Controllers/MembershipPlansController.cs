using FitnessStudioApi.DTOs.MembershipPlan;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public sealed class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;

    public MembershipPlansController(IMembershipPlanService service)
    {
        _service = service;
    }

    /// <summary>List all membership plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MembershipPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var plans = await _service.GetAllAsync(isActive);
        return Ok(plans);
    }

    /// <summary>Get a membership plan by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        return Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipPlanDto dto)
    {
        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto)
    {
        var plan = await _service.UpdateAsync(id, dto);
        return Ok(plan);
    }

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
