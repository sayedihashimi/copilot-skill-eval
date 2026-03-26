using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController(IInstructorService service) : ControllerBase
{
    /// <summary>List instructors with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InstructorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var instructors = await service.GetAllAsync(specialization, isActive, ct);
        return Ok(instructors);
    }

    /// <summary>Get instructor details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<InstructorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var instructor = await service.GetByIdAsync(id, ct);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create a new instructor.</summary>
    [HttpPost]
    [ProducesResponseType<InstructorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto, CancellationToken ct)
    {
        var instructor = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an existing instructor.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<InstructorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto, CancellationToken ct)
    {
        var instructor = await service.UpdateAsync(id, dto, ct);
        return Ok(instructor);
    }

    /// <summary>Get instructor's class schedule.</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleListDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(
        int id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var schedule = await service.GetScheduleAsync(id, fromDate, toDate, ct);
        return Ok(schedule);
    }
}
