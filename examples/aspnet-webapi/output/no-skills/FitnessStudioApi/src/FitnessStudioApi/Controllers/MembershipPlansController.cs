using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;

    public MembershipPlansController(IMembershipPlanService service) => _service = service;

    /// <summary>List all membership plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<MembershipPlanDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isActive = null)
        => Ok(await _service.GetAllAsync(page, pageSize, isActive));

    /// <summary>Get membership plan by ID</summary>
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

    /// <summary>Update a membership plan</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MembershipPlanDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipPlanDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deactivate a membership plan</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
