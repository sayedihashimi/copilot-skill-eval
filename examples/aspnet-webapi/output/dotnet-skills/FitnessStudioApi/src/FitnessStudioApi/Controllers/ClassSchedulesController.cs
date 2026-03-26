using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.ClassSchedule;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public sealed class ClassSchedulesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassSchedulesController(IClassScheduleService service)
    {
        _service = service;
    }

    /// <summary>List scheduled classes with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ClassScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get class details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await _service.GetByIdAsync(id);
        return Ok(schedule);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto)
    {
        var schedule = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
    {
        var schedule = await _service.UpdateAsync(id, dto);
        return Ok(schedule);
    }

    /// <summary>Cancel a class and all its bookings</summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
    {
        var schedule = await _service.CancelAsync(id, dto);
        return Ok(schedule);
    }

    /// <summary>Get the confirmed roster for a class</summary>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(List<RosterEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoster(int id)
    {
        var roster = await _service.GetRosterAsync(id);
        return Ok(roster);
    }

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    [ProducesResponseType(typeof(List<RosterEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWaitlist(int id)
    {
        var waitlist = await _service.GetWaitlistAsync(id);
        return Ok(waitlist);
    }

    /// <summary>Get available classes in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<ClassScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable()
    {
        var classes = await _service.GetAvailableClassesAsync();
        return Ok(classes);
    }
}
