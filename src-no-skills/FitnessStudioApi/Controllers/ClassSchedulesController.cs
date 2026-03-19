using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassSchedulesController(IClassScheduleService service)
    {
        _service = service;
    }

    /// <summary>List scheduled classes with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? available,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, available, pagination);
        return Ok(result);
    }

    /// <summary>Get class details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var cs = await _service.GetByIdAsync(id);
        return cs == null ? NotFound() : Ok(cs);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto)
    {
        var cs = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = cs.Id }, cs);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
    {
        var cs = await _service.UpdateAsync(id, dto);
        return cs == null ? NotFound() : Ok(cs);
    }

    /// <summary>Cancel a class (cascades to all bookings)</summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
    {
        var cs = await _service.CancelAsync(id, dto);
        return Ok(cs);
    }

    /// <summary>Get confirmed members roster for a class</summary>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(List<ClassRosterItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetRoster(int id)
    {
        var result = await _service.GetRosterAsync(id);
        return Ok(result);
    }

    /// <summary>Get waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    [ProducesResponseType(typeof(List<ClassRosterItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetWaitlist(int id)
    {
        var result = await _service.GetWaitlistAsync(id);
        return Ok(result);
    }

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), 200)]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _service.GetAvailableAsync();
        return Ok(result);
    }
}
