using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController(IClassScheduleService service) : ControllerBase
{
    /// <summary>List scheduled classes with filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<ClassScheduleListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get class details including roster and waitlist counts.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassScheduleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var schedule = await service.GetByIdAsync(id, ct);
        return schedule is null ? NotFound() : Ok(schedule);
    }

    /// <summary>Schedule a new class.</summary>
    [HttpPost]
    [ProducesResponseType<ClassScheduleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto, CancellationToken ct)
    {
        var schedule = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    /// <summary>Update class details.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassScheduleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto, CancellationToken ct)
    {
        var schedule = await service.UpdateAsync(id, dto, ct);
        return Ok(schedule);
    }

    /// <summary>Cancel a class and all its bookings.</summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType<ClassScheduleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto, CancellationToken ct)
    {
        var schedule = await service.CancelAsync(id, dto, ct);
        return Ok(schedule);
    }

    /// <summary>Get the roster of confirmed/attended members for a class.</summary>
    [HttpGet("{id:int}/roster")]
    [ProducesResponseType<IReadOnlyList<RosterEntryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoster(int id, CancellationToken ct)
    {
        var roster = await service.GetRosterAsync(id, ct);
        return Ok(roster);
    }

    /// <summary>Get the waitlist for a class.</summary>
    [HttpGet("{id:int}/waitlist")]
    [ProducesResponseType<IReadOnlyList<WaitlistEntryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWaitlist(int id, CancellationToken ct)
    {
        var waitlist = await service.GetWaitlistAsync(id, ct);
        return Ok(waitlist);
    }

    /// <summary>Get classes with available spots in the next 7 days.</summary>
    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(CancellationToken ct)
    {
        var available = await service.GetAvailableAsync(ct);
        return Ok(available);
    }
}
