using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/veterinarians")]
[Produces("application/json")]
public class VeterinariansController(IVeterinarianService vetService) : ControllerBase
{
    /// <summary>List all veterinarians with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VeterinarianDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await vetService.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get veterinarian details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var vet = await vetService.GetByIdAsync(id, ct);
        return vet is null ? NotFound() : Ok(vet);
    }

    /// <summary>Create a new veterinarian.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVeterinarianDto dto, CancellationToken ct)
    {
        var vet = await vetService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    /// <summary>Update veterinarian info.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VeterinarianDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVeterinarianDto dto, CancellationToken ct)
    {
        var vet = await vetService.UpdateAsync(id, dto, ct);
        return vet is null ? NotFound() : Ok(vet);
    }

    /// <summary>Get vet's appointments for a specific date.</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var appointments = await vetService.GetScheduleAsync(id, date, ct);
        return Ok(appointments);
    }

    /// <summary>Get all appointments for a vet with pagination and optional status filter.</summary>
    [HttpGet("{id:int}/appointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments(
        int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await vetService.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return Ok(result);
    }
}
