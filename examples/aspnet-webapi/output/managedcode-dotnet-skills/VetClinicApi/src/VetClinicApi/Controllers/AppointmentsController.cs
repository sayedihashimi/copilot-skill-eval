using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/appointments")]
[Produces("application/json")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    /// <summary>List appointments with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
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

    /// <summary>Get appointment details including pet, vet, and medical record.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var appointment = await appointmentService.GetByIdAsync(id, ct);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Schedule a new appointment with conflict detection.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto, CancellationToken ct)
    {
        var appointment = await appointmentService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    /// <summary>Update appointment details (re-checks conflicts on date/time changes).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto, CancellationToken ct)
    {
        var appointment = await appointmentService.UpdateAsync(id, dto, ct);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Update appointment status (enforces workflow rules).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto, CancellationToken ct)
    {
        var appointment = await appointmentService.UpdateStatusAsync(id, dto, ct);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Get all of today's appointments.</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var appointments = await appointmentService.GetTodayAsync(ct);
        return Ok(appointments);
    }
}
