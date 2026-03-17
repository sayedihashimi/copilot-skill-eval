using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    private readonly IAppointmentService _appointmentService = appointmentService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _appointmentService.GetAllAsync(dateFrom, dateTo, status, vetId, petId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetById(int id, CancellationToken ct)
    {
        var appointment = await _appointmentService.GetByIdAsync(id, ct);
        return appointment is not null ? Ok(appointment) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await _appointmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> Update(int id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await _appointmentService.UpdateAsync(id, request, ct);
        return appointment is not null ? Ok(appointment) : NotFound();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(
        int id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await _appointmentService.UpdateStatusAsync(id, request, ct);
        return appointment is not null ? Ok(appointment) : NotFound();
    }

    [HttpGet("today")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetToday(CancellationToken ct)
    {
        var appointments = await _appointmentService.GetTodayAsync(ct);
        return Ok(appointments);
    }
}
