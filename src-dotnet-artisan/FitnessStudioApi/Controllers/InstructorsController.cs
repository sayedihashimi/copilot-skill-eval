using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController(IInstructorService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InstructorResponse>>(200)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isActive)
        => Ok(await service.GetAllAsync(specialization, isActive));

    [HttpGet("{id:int}")]
    [ProducesResponseType<InstructorResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<InstructorResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateInstructorRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<InstructorResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateInstructorRequest request)
        => Ok(await service.UpdateAsync(id, request));

    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleResponse>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await service.GetScheduleAsync(id, from, to));
}
