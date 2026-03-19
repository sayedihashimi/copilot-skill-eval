using Microsoft.AspNetCore.Mvc;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Controllers;

[ApiController]
[Route("api/classes")]
[Produces("application/json")]
public class ClassesController(IClassScheduleService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<ClassScheduleResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int? classTypeId, [FromQuery] int? instructorId,
        [FromQuery] bool? hasAvailability, [FromQuery] PaginationParams pagination)
        => Ok(await service.GetAllAsync(from, to, classTypeId, instructorId, hasAvailability, pagination));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClassScheduleResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<ClassScheduleResponse>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(CreateClassScheduleRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ClassScheduleResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateClassScheduleRequest request)
        => Ok(await service.UpdateAsync(id, request));

    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType<ClassScheduleResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, CancelClassRequest request)
        => Ok(await service.CancelClassAsync(id, request));

    [HttpGet("{id:int}/roster")]
    [ProducesResponseType<IReadOnlyList<RosterEntry>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRoster(int id)
        => Ok(await service.GetRosterAsync(id));

    [HttpGet("{id:int}/waitlist")]
    [ProducesResponseType<IReadOnlyList<RosterEntry>>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWaitlist(int id)
        => Ok(await service.GetWaitlistAsync(id));

    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyList<ClassScheduleResponse>>(200)]
    public async Task<IActionResult> GetAvailable()
        => Ok(await service.GetAvailableAsync());
}
