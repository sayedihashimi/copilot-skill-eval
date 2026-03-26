using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentsController(IAppointmentService service)
    {
        _service = service;
    }

    /// <summary>List appointments with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] int? vetId,
        [FromQuery] int? petId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(fromDate, toDate, status, vetId, petId,
            new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get appointment details with pet, vet, and medical record</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _service.GetByIdAsync(id);
        return appointment == null ? NotFound() : Ok(appointment);
    }

    /// <summary>Schedule a new appointment with conflict detection</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        var appointment = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    /// <summary>Update appointment details (date/time changes re-check conflicts)</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
    {
        var appointment = await _service.UpdateAsync(id, dto);
        return appointment == null ? NotFound() : Ok(appointment);
    }

    /// <summary>Update appointment status with workflow rules enforcement</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
    {
        var appointment = await _service.UpdateStatusAsync(id, dto);
        return appointment == null ? NotFound() : Ok(appointment);
    }

    /// <summary>Get all of today's appointments</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetToday()
    {
        var appointments = await _service.GetTodayAsync();
        return Ok(appointments);
    }
}
