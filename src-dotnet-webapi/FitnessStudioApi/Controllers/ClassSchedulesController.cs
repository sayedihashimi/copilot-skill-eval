using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassSchedulesController(IClassScheduleService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ClassScheduleResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List class schedules")]
    [EndpointDescription("Returns a paginated list of class schedules with filters for date range, class type, instructor, and availability.")]
    public async Task<ActionResult<PagedResponse<ClassScheduleResponse>>> GetAll(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? classTypeId,
        [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ClassScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get class schedule details")]
    [EndpointDescription("Returns full details of a specific class schedule including enrollment, waitlist, and availability.")]
    public async Task<ActionResult<ClassScheduleResponse>> GetById(int id, CancellationToken ct)
    {
        var schedule = await service.GetByIdAsync(id, ct);
        return schedule is null ? NotFound() : Ok(schedule);
    }

    [HttpPost]
    [ProducesResponseType<ClassScheduleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Schedule a new class")]
    [EndpointDescription("Creates a new class schedule. Validates instructor availability and prevents scheduling conflicts.")]
    public async Task<ActionResult<ClassScheduleResponse>> Create(
        CreateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<ClassScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a class schedule")]
    [EndpointDescription("Updates an existing class schedule. Re-validates instructor conflicts.")]
    public async Task<ActionResult<ClassScheduleResponse>> Update(
        int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await service.UpdateAsync(id, request, ct);
        return Ok(schedule);
    }

    [HttpPatch("{id}/cancel")]
    [ProducesResponseType<ClassScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel a class")]
    [EndpointDescription("Cancels a scheduled class and automatically cancels all associated bookings with reason 'Class cancelled by studio'.")]
    public async Task<ActionResult<ClassScheduleResponse>> Cancel(
        int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await service.CancelClassAsync(id, request, ct);
        return Ok(schedule);
    }

    [HttpGet("{id}/roster")]
    [ProducesResponseType<List<ClassRosterResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get class roster")]
    [EndpointDescription("Returns the list of confirmed and attended members for a class.")]
    public async Task<ActionResult<List<ClassRosterResponse>>> GetRoster(int id, CancellationToken ct)
    {
        var result = await service.GetRosterAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("{id}/waitlist")]
    [ProducesResponseType<List<ClassRosterResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get class waitlist")]
    [EndpointDescription("Returns the waitlisted members for a class, ordered by position.")]
    public async Task<ActionResult<List<ClassRosterResponse>>> GetWaitlist(int id, CancellationToken ct)
    {
        var result = await service.GetWaitlistAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("available")]
    [ProducesResponseType<List<ClassScheduleResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("Get available classes")]
    [EndpointDescription("Returns all scheduled classes with available spots in the next 7 days.")]
    public async Task<ActionResult<List<ClassScheduleResponse>>> GetAvailable(CancellationToken ct)
    {
        var result = await service.GetAvailableClassesAsync(ct);
        return Ok(result);
    }
}
