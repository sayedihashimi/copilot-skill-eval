using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassSchedulesController : ControllerBase
{
    private readonly IClassScheduleService _service;

    public ClassSchedulesController(IClassScheduleService service) => _service = service;

    /// <summary>List scheduled classes with filters and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ClassScheduleListDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] int? classTypeId = null, [FromQuery] int? instructorId = null,
        [FromQuery] bool? available = null)
        => Ok(await _service.GetAllAsync(page, pageSize, from, to, classTypeId, instructorId, available));

    /// <summary>Get class details including availability</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var cs = await _service.GetByIdAsync(id);
        return cs == null ? NotFound() : Ok(cs);
    }

    /// <summary>Schedule a new class</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassScheduleDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateClassScheduleDto dto)
    {
        var cs = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = cs.Id }, cs);
    }

    /// <summary>Update class details</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassScheduleDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Cancel a class and all its bookings</summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ClassScheduleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelClassDto dto)
        => Ok(await _service.CancelAsync(id, dto));

    /// <summary>Get the roster of confirmed members for a class</summary>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(IEnumerable<RosterEntryDto>), 200)]
    public async Task<IActionResult> GetRoster(int id)
        => Ok(await _service.GetRosterAsync(id));

    /// <summary>Get the waitlist for a class</summary>
    [HttpGet("{id}/waitlist")]
    [ProducesResponseType(typeof(IEnumerable<WaitlistEntryDto>), 200)]
    public async Task<IActionResult> GetWaitlist(int id)
        => Ok(await _service.GetWaitlistAsync(id));

    /// <summary>Get classes with available spots in the next 7 days</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<ClassScheduleListDto>), 200)]
    public async Task<IActionResult> GetAvailable()
        => Ok(await _service.GetAvailableAsync());
}
