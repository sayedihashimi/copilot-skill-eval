using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/owners")]
[Produces("application/json")]
public class OwnersController(IOwnerService ownerService) : ControllerBase
{
    /// <summary>List all owners with optional search and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await ownerService.GetAllAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get an owner by ID including their pets.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var owner = await ownerService.GetByIdAsync(id, ct);
        return owner is null ? NotFound() : Ok(owner);
    }

    /// <summary>Create a new owner.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOwnerDto dto, CancellationToken ct)
    {
        var owner = await ownerService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    /// <summary>Update an existing owner.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOwnerDto dto, CancellationToken ct)
    {
        var owner = await ownerService.UpdateAsync(id, dto, ct);
        return owner is null ? NotFound() : Ok(owner);
    }

    /// <summary>Delete an owner (fails if owner has active pets).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await ownerService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get all pets for an owner.</summary>
    [HttpGet("{id:int}/pets")]
    [ProducesResponseType(typeof(List<PetSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPets(int id, CancellationToken ct)
    {
        var pets = await ownerService.GetPetsAsync(id, ct);
        return Ok(pets);
    }

    /// <summary>Get appointment history for all of an owner's pets.</summary>
    [HttpGet("{id:int}/appointments")]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments(int id, CancellationToken ct)
    {
        var appointments = await ownerService.GetAppointmentsAsync(id, ct);
        return Ok(appointments);
    }
}
