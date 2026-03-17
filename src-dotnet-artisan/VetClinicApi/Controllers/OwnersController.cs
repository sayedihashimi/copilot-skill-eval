using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OwnersController(IOwnerService ownerService) : ControllerBase
{
    private readonly IOwnerService _ownerService = ownerService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<OwnerDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _ownerService.GetAllAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDetailDto>> GetById(int id, CancellationToken ct)
    {
        var owner = await _ownerService.GetByIdAsync(id, ct);
        return owner is not null ? Ok(owner) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<OwnerDto>> Create([FromBody] CreateOwnerRequest request, CancellationToken ct)
    {
        var owner = await _ownerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OwnerDto>> Update(int id, [FromBody] UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await _ownerService.UpdateAsync(id, request, ct);
        return owner is not null ? Ok(owner) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _ownerService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/pets")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetPets(int id, CancellationToken ct)
    {
        var pets = await _ownerService.GetPetsAsync(id, ct);
        return Ok(pets);
    }

    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetAppointments(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _ownerService.GetAppointmentsAsync(id, page, pageSize, ct);
        return Ok(result);
    }
}
