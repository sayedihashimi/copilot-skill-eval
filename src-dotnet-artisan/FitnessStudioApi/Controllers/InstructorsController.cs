using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
public sealed class InstructorsController(IInstructorService service) : ControllerBase
{
    private readonly IInstructorService _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstructorResponse>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var instructors = await _service.GetAllAsync(specialization, isActive, ct);
        return Ok(instructors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstructorResponse>> GetById(int id, CancellationToken ct)
    {
        var instructor = await _service.GetByIdAsync(id, ct);
        return Ok(instructor);
    }

    [HttpPost]
    public async Task<ActionResult<InstructorResponse>> Create(CreateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InstructorResponse>> Update(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await _service.UpdateAsync(id, request, ct);
        return Ok(instructor);
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<ClassScheduleResponse>>> GetSchedule(
        int id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var schedule = await _service.GetScheduleAsync(id, from, to, ct);
        return Ok(schedule);
    }
}
