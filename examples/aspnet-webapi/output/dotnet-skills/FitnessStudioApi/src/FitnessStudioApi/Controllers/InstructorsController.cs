using FitnessStudioApi.DTOs.ClassSchedule;
using FitnessStudioApi.DTOs.Instructor;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public sealed class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service)
    {
        _service = service;
    }

    /// <summary>List instructors with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InstructorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isActive)
    {
        var instructors = await _service.GetAllAsync(specialization, isActive);
        return Ok(instructors);
    }

    /// <summary>Get instructor details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _service.GetByIdAsync(id);
        return Ok(instructor);
    }

    /// <summary>Create a new instructor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto)
    {
        var instructor = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update an instructor</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto)
    {
        var instructor = await _service.UpdateAsync(id, dto);
        return Ok(instructor);
    }

    /// <summary>Get instructor's class schedule</summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(
        int id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var schedule = await _service.GetScheduleAsync(id, fromDate, toDate);
        return Ok(schedule);
    }
}
