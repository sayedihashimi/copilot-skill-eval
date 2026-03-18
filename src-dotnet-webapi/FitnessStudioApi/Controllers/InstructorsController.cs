using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController(IInstructorService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<InstructorResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List instructors")]
    [EndpointDescription("Returns a paginated list of instructors with optional filters for specialization and active status.")]
    public async Task<ActionResult<PagedResponse<InstructorResponse>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(specialization, isActive, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<InstructorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get instructor details")]
    [EndpointDescription("Returns full details of a specific instructor.")]
    public async Task<ActionResult<InstructorResponse>> GetById(int id, CancellationToken ct)
    {
        var instructor = await service.GetByIdAsync(id, ct);
        return instructor is null ? NotFound() : Ok(instructor);
    }

    [HttpPost]
    [ProducesResponseType<InstructorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new instructor")]
    [EndpointDescription("Creates a new instructor profile with the provided details.")]
    public async Task<ActionResult<InstructorResponse>> Create(
        CreateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<InstructorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update an instructor")]
    [EndpointDescription("Updates an existing instructor's profile information.")]
    public async Task<ActionResult<InstructorResponse>> Update(
        int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await service.UpdateAsync(id, request, ct);
        return Ok(instructor);
    }

    [HttpGet("{id}/schedule")]
    [ProducesResponseType<List<ClassScheduleResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get instructor schedule")]
    [EndpointDescription("Returns the class schedule for a specific instructor with optional date range filter.")]
    public async Task<ActionResult<List<ClassScheduleResponse>>> GetSchedule(
        int id,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken ct = default)
    {
        var result = await service.GetScheduleAsync(id, fromDate, toDate, ct);
        return Ok(result);
    }
}
