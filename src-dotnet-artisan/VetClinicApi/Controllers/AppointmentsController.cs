using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentResponse>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await appointmentService.GetAllAsync(fromDate, toDate, status, vetId, petId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDetailResponse>> GetById(int id, CancellationToken ct = default)
    {
        var appointment = await appointmentService.GetByIdAsync(id, ct);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create(CreateAppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = await appointmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> Update(int id, UpdateAppointmentRequest request, CancellationToken ct = default)
    {
        var result = await appointmentService.UpdateAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<AppointmentResponse>> UpdateStatus(int id, UpdateAppointmentStatusRequest request, CancellationToken ct = default)
    {
        var result = await appointmentService.UpdateStatusAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("today")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> GetToday(CancellationToken ct = default)
    {
        var appointments = await appointmentService.GetTodayAsync(ct);
        return Ok(appointments);
    }
}
