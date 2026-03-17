using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VeterinariansController(IVeterinarianService veterinarianService) : ControllerBase
{
    private readonly IVeterinarianService _veterinarianService = veterinarianService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<VeterinarianDto>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _veterinarianService.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeterinarianDto>> GetById(int id, CancellationToken ct)
    {
        var vet = await _veterinarianService.GetByIdAsync(id, ct);
        return vet is not null ? Ok(vet) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<VeterinarianDto>> Create([FromBody] CreateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await _veterinarianService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VeterinarianDto>> Update(int id, [FromBody] UpdateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await _veterinarianService.UpdateAsync(id, request, ct);
        return vet is not null ? Ok(vet) : NotFound();
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetSchedule(
        int id,
        [FromQuery] DateOnly? date,
        CancellationToken ct)
    {
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = await _veterinarianService.GetScheduleAsync(id, scheduleDate, ct);
        return Ok(schedule);
    }

    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetAppointments(
        int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _veterinarianService.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return Ok(result);
    }
}
