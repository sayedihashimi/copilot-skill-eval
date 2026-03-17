using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
public sealed class MembershipPlansController(IMembershipPlanService service) : ControllerBase
{
    private readonly IMembershipPlanService _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MembershipPlanResponse>>> GetAll(CancellationToken ct)
    {
        var plans = await _service.GetAllActivePlansAsync(ct);
        return Ok(plans);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipPlanResponse>> GetById(int id, CancellationToken ct)
    {
        var plan = await _service.GetByIdAsync(id, ct);
        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<MembershipPlanResponse>> Create(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MembershipPlanResponse>> Update(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await _service.UpdateAsync(id, request, ct);
        return Ok(plan);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, ct);
        return NoContent();
    }
}
