using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
public sealed class ClassSchedulesController(IClassScheduleService service) : ControllerBase
{
    private readonly IClassScheduleService _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClassScheduleResponse>>> GetAll(
        [FromQuery] DateTime? date,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(date, classTypeId, instructorId, hasAvailability, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClassScheduleResponse>> GetById(int id, CancellationToken ct)
    {
        var schedule = await _service.GetByIdAsync(id, ct);
        return Ok(schedule);
    }

    [HttpPost]
    public async Task<ActionResult<ClassScheduleResponse>> Create(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClassScheduleResponse>> Update(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await _service.UpdateAsync(id, request, ct);
        return Ok(schedule);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ClassScheduleResponse>> CancelClass(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await _service.CancelClassAsync(id, request, ct);
        return Ok(schedule);
    }

    [HttpGet("{id:int}/roster")]
    public async Task<ActionResult<IReadOnlyList<RosterEntryResponse>>> GetRoster(int id, CancellationToken ct)
    {
        var roster = await _service.GetRosterAsync(id, ct);
        return Ok(roster);
    }

    [HttpGet("{id:int}/waitlist")]
    public async Task<ActionResult<IReadOnlyList<RosterEntryResponse>>> GetWaitlist(int id, CancellationToken ct)
    {
        var waitlist = await _service.GetWaitlistAsync(id, ct);
        return Ok(waitlist);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<ClassScheduleResponse>>> GetAvailable(CancellationToken ct)
    {
        var classes = await _service.GetAvailableClassesAsync(ct);
        return Ok(classes);
    }
}
