using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List all appointments")]
    [EndpointDescription("Returns a paginated list of appointments. Supports filtering by date range, status, veterinarian, and pet.")]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> GetAll(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await appointmentService.GetAllAsync(dateFrom, dateTo, status, vetId, petId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get appointment by ID")]
    [EndpointDescription("Returns appointment details including pet, veterinarian, and medical record if available.")]
    public async Task<ActionResult<AppointmentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Schedule a new appointment")]
    [EndpointDescription("Creates a new appointment. Enforces scheduling conflict detection for the veterinarian.")]
    public async Task<ActionResult<AppointmentResponse>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an appointment")]
    [EndpointDescription("Updates appointment details. Re-checks scheduling conflicts.")]
    public async Task<ActionResult<AppointmentResponse>> Update(
        int id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.UpdateAsync(id, request, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update appointment status")]
    [EndpointDescription("Updates the appointment status following the allowed workflow transitions. Cancellation requires a reason.")]
    public async Task<ActionResult<AppointmentResponse>> UpdateStatus(
        int id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.UpdateStatusAsync(id, request, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpGet("today")]
    [ProducesResponseType<List<AppointmentResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("Get today's appointments")]
    [EndpointDescription("Returns all appointments scheduled for today, ordered by time.")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetToday(CancellationToken cancellationToken)
    {
        var appointments = await appointmentService.GetTodayAsync(cancellationToken);
        return Ok(appointments);
    }
}
