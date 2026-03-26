using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
[Produces("application/json")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _service;

    public InstructorsController(IInstructorService service) => _service = service;

    /// <summary>List instructors with filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<InstructorDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? specialization = null, [FromQuery] bool? isActive = null)
        => Ok(await _service.GetAllAsync(page, pageSize, specialization, isActive));

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

    /// <summary>Update an instructor</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(InstructorDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Get instructor's class schedule</summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(IEnumerable<ClassScheduleListDto>), 200)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        => Ok(await _service.GetScheduleAsync(id, from, to));
}
