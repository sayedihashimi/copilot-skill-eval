using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service)
    {
        _service = service;
    }

    /// <summary>List instructors with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<InstructorDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? specialization, [FromQuery] bool? isActive)
    {
        var result = await _service.GetAllAsync(specialization, isActive);
        return Ok(result);
    }

    /// <summary>Get instructor details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InstructorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _service.GetByIdAsync(id);
        return instructor == null ? NotFound() : Ok(instructor);
    }

    /// <summary>Create a new instructor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InstructorDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto)
    {
        var instructor = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    /// <summary>Update instructor</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InstructorDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto)
    {
        var instructor = await _service.UpdateAsync(id, dto);
        return instructor == null ? NotFound() : Ok(instructor);
    }

    /// <summary>Get instructor's class schedule</summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _service.GetScheduleAsync(id, fromDate, toDate);
        return Ok(result);
    }
}
