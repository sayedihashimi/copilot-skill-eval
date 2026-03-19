using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController(IMembershipPlanService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MembershipPlanResponse>>(200)]
    public async Task<IActionResult> GetAll()
        => Ok(await service.GetAllActiveAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<MembershipPlanResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<MembershipPlanResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateMembershipPlanRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<MembershipPlanResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateMembershipPlanRequest request)
        => Ok(await service.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
