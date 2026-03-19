using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;

    public MembershipPlansController(IMembershipPlanService service)
    {
        _service = service;
    }

    /// <summary>List all active membership plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MembershipPlanDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _service.GetAllAsync();
        return Ok(plans);
    }

    /// <summary>Get plan details by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);
        return plan == null ? NotFound() : Ok(plan);
    }

    /// <summary>Create a new membership plan</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MembershipPlanDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipPlanDto dto)
    {
        var plan = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    /// <summary>Update an existing membership plan</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto)
    {
        var plan = await _service.UpdateAsync(id, dto);
        return plan == null ? NotFound() : Ok(plan);
    }

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return result ? NoContent() : NotFound();
    }
}
