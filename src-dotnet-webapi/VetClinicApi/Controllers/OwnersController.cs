using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController(IOwnerService ownerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<OwnerSummaryResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List all owners")]
    [EndpointDescription("Returns a paginated list of owners. Supports searching by name or email.")]
    public async Task<ActionResult<PagedResponse<OwnerSummaryResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await ownerService.GetAllAsync(search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<OwnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get owner by ID")]
    [EndpointDescription("Returns owner details including their pets.")]
    public async Task<ActionResult<OwnerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var owner = await ownerService.GetByIdAsync(id, cancellationToken);
        return owner is null ? NotFound() : Ok(owner);
    }

    [HttpPost]
    [ProducesResponseType<OwnerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new owner")]
    [EndpointDescription("Creates a new pet owner. Email must be unique.")]
    public async Task<ActionResult<OwnerResponse>> Create(
        [FromBody] CreateOwnerRequest request,
        CancellationToken cancellationToken)
    {
        var owner = await ownerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<OwnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an owner")]
    [EndpointDescription("Updates owner information. Email must remain unique.")]
    public async Task<ActionResult<OwnerResponse>> Update(
        int id,
        [FromBody] UpdateOwnerRequest request,
        CancellationToken cancellationToken)
    {
        var owner = await ownerService.UpdateAsync(id, request, cancellationToken);
        return owner is null ? NotFound() : Ok(owner);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete an owner")]
    [EndpointDescription("Deletes an owner. Fails if the owner has active pets.")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await ownerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/pets")]
    [ProducesResponseType<List<PetSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get owner's pets")]
    [EndpointDescription("Returns all pets belonging to the specified owner.")]
    public async Task<ActionResult<List<PetSummaryResponse>>> GetPets(int id, CancellationToken cancellationToken)
    {
        var pets = await ownerService.GetPetsAsync(id, cancellationToken);
        return Ok(pets);
    }

    [HttpGet("{id}/appointments")]
    [ProducesResponseType<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get owner's appointment history")]
    [EndpointDescription("Returns appointment history for all of the owner's pets.")]
    public async Task<ActionResult<PagedResponse<AppointmentResponse>>> GetAppointments(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await ownerService.GetAppointmentsAsync(id, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
