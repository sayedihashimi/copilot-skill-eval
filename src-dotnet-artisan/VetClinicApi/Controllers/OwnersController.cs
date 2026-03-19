using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OwnersController(IOwnerService ownerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OwnerResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await ownerService.GetAllAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDetailResponse>> GetById(int id, CancellationToken ct = default)
    {
        var owner = await ownerService.GetByIdAsync(id, ct);
        return owner is null ? NotFound() : Ok(owner);
    }

    [HttpPost]
    public async Task<ActionResult<OwnerResponse>> Create(CreateOwnerRequest request, CancellationToken ct = default)
    {
        var owner = await ownerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OwnerResponse>> Update(int id, UpdateOwnerRequest request, CancellationToken ct = default)
    {
        var owner = await ownerService.UpdateAsync(id, request, ct);
        return owner is null ? NotFound() : Ok(owner);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var deleted = await ownerService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/pets")]
    public async Task<ActionResult<IReadOnlyList<PetResponse>>> GetPets(int id, CancellationToken ct = default)
    {
        if (!await OwnerExistsAsync(id, ct))
        {
            return NotFound();
        }

        var pets = await ownerService.GetPetsAsync(id, ct);
        return Ok(pets);
    }

    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> GetAppointments(int id, CancellationToken ct = default)
    {
        if (!await OwnerExistsAsync(id, ct))
        {
            return NotFound();
        }

        var appointments = await ownerService.GetAppointmentsAsync(id, ct);
        return Ok(appointments);
    }

    private async Task<bool> OwnerExistsAsync(int id, CancellationToken ct) =>
        await ownerService.GetByIdAsync(id, ct) is not null;
}
