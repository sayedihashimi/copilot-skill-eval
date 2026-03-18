using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariansController(IVeterinarianService vetService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<VeterinarianResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List all veterinarians")]
    [EndpointDescription("Returns a paginated list of veterinarians. Supports filtering by specialization and availability.")]
    public async Task<ActionResult<PagedResponse<VeterinarianResponse>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await vetService.GetAllAsync(specialization, isAvailable, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<VeterinarianResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get veterinarian by ID")]
    [EndpointDescription("Returns veterinarian details.")]
    public async Task<ActionResult<VeterinarianResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var vet = await vetService.GetByIdAsync(id, cancellationToken);
        return vet is null ? NotFound() : Ok(vet);
    }

    [HttpPost]
    [ProducesResponseType<VeterinarianResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new veterinarian")]
    [EndpointDescription("Creates a new veterinarian. Email and license number must be unique.")]
    public async Task<ActionResult<VeterinarianResponse>> Create(
        [FromBody] CreateVeterinarianRequest request,
        CancellationToken cancellationToken)
    {
        var vet = await vetService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<VeterinarianResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a veterinarian")]
    [EndpointDescription("Updates veterinarian information.")]
    public async Task<ActionResult<VeterinarianResponse>> Update(
        int id,
        [FromBody] UpdateVeterinarianRequest request,
        CancellationToken cancellationToken)
    {
        var vet = await vetService.UpdateAsync(id, request, cancellationToken);
        return vet is null ? NotFound() : Ok(vet);
    }

    [HttpGet("{id}/schedule")]
    [ProducesResponseType<List<AppointmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get veterinarian's daily schedule")]
    [EndpointDescription("Returns all non-cancelled appointments for the specified veterinarian on the given date.")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetSchedule(
        int id,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var schedule = await vetService.GetScheduleAsync(id, date, cancellationToken);
        return Ok(schedule);
    }

    [HttpGet("{id}/appointments")]
    [ProducesResponseType<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get veterinarian's appointments")]
    [EndpointDescription("Returns a paginated list of appointments for the specified veterinarian. Supports filtering by status.")]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> GetAppointments(
        int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await vetService.GetAppointmentsAsync(id, status, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
