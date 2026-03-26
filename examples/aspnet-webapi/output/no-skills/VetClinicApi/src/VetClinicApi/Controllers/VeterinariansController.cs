using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariansController : ControllerBase
{
    private readonly IVeterinarianService _service;

    public VeterinariansController(IVeterinarianService service)
    {
        _service = service;
    }

    /// <summary>List all veterinarians with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VeterinarianResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? specialization,
        [FromQuery] bool? isAvailable,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(specialization, isAvailable, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get veterinarian details</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var vet = await _service.GetByIdAsync(id);
        return vet == null ? NotFound() : Ok(vet);
    }

    /// <summary>Create a new veterinarian</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] VeterinarianCreateDto dto)
    {
        var vet = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = vet.Id }, vet);
    }

    /// <summary>Update veterinarian info</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VeterinarianResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] VeterinarianUpdateDto dto)
    {
        var vet = await _service.UpdateAsync(id, dto);
        return vet == null ? NotFound() : Ok(vet);
    }

    /// <summary>Get vet's appointments for a specific date</summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateOnly date)
    {
        var schedule = await _service.GetScheduleAsync(id, date);
        return Ok(schedule);
    }

    /// <summary>Get all appointments for a vet with pagination and status filter</summary>
    [HttpGet("{id}/appointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAppointmentsAsync(id, status, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }
}
