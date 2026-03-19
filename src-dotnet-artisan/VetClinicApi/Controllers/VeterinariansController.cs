using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VeterinariansController(IVeterinarianService vetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<VeterinarianResponse>>> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await vetService.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeterinarianResponse>> GetById(int id, CancellationToken ct = default)
    {
        var vet = await vetService.GetByIdAsync(id, ct);
        return vet is null ? NotFound() : Ok(vet);
    }

    [HttpPost]
    public async Task<ActionResult<VeterinarianResponse>> Create(CreateVeterinarianRequest request, CancellationToken ct = default)
    {
        var vet = await vetService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VeterinarianResponse>> Update(int id, UpdateVeterinarianRequest request, CancellationToken ct = default)
    {
        var vet = await vetService.UpdateAsync(id, request, ct);
        return vet is null ? NotFound() : Ok(vet);
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> GetSchedule(
        int id,
        [FromQuery] DateOnly date,
        CancellationToken ct = default)
    {
        var vet = await vetService.GetByIdAsync(id, ct);
        if (vet is null)
        {
            return NotFound();
        }

        var schedule = await vetService.GetScheduleAsync(id, date, ct);
        return Ok(schedule);
    }

    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PagedResult<AppointmentResponse>>> GetAppointments(
        int id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var vet = await vetService.GetByIdAsync(id, ct);
        if (vet is null)
        {
            return NotFound();
        }

        var result = await vetService.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return Ok(result);
    }
}
