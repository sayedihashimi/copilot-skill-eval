using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController(IPetService petService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<PetResponse>>(StatusCodes.Status200OK)]
    [EndpointSummary("List all pets")]
    [EndpointDescription("Returns a paginated list of active pets. Supports searching by name, filtering by species, and optionally including inactive pets.")]
    public async Task<ActionResult<PagedResponse<PetResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await petService.GetAllAsync(search, species, includeInactive, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get pet by ID")]
    [EndpointDescription("Returns pet details including owner information.")]
    public async Task<ActionResult<PetResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var pet = await petService.GetByIdAsync(id, cancellationToken);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    [ProducesResponseType<PetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new pet")]
    [EndpointDescription("Creates a new pet record. Species must be Dog, Cat, Bird, or Rabbit. Owner must exist.")]
    public async Task<ActionResult<PetResponse>> Create(
        [FromBody] CreatePetRequest request,
        CancellationToken cancellationToken)
    {
        var pet = await petService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<PetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a pet")]
    [EndpointDescription("Updates pet information. Changing OwnerId transfers ownership.")]
    public async Task<ActionResult<PetResponse>> Update(
        int id,
        [FromBody] UpdatePetRequest request,
        CancellationToken cancellationToken)
    {
        var pet = await petService.UpdateAsync(id, request, cancellationToken);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Soft delete a pet")]
    [EndpointDescription("Marks a pet as inactive (soft delete). The pet record is preserved.")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await petService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/medical-records")]
    [ProducesResponseType<List<MedicalRecordResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get pet's medical records")]
    [EndpointDescription("Returns all medical records for the specified pet, including prescriptions.")]
    public async Task<ActionResult<List<MedicalRecordResponse>>> GetMedicalRecords(int id, CancellationToken cancellationToken)
    {
        var records = await petService.GetMedicalRecordsAsync(id, cancellationToken);
        return Ok(records);
    }

    [HttpGet("{id}/vaccinations")]
    [ProducesResponseType<List<VaccinationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get pet's vaccinations")]
    [EndpointDescription("Returns all vaccination records for the specified pet.")]
    public async Task<ActionResult<List<VaccinationResponse>>> GetVaccinations(int id, CancellationToken cancellationToken)
    {
        var vaccinations = await petService.GetVaccinationsAsync(id, cancellationToken);
        return Ok(vaccinations);
    }

    [HttpGet("{id}/vaccinations/upcoming")]
    [ProducesResponseType<List<VaccinationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get upcoming/overdue vaccinations")]
    [EndpointDescription("Returns vaccinations that are due soon (within 30 days) or already expired.")]
    public async Task<ActionResult<List<VaccinationResponse>>> GetUpcomingVaccinations(int id, CancellationToken cancellationToken)
    {
        var vaccinations = await petService.GetUpcomingVaccinationsAsync(id, cancellationToken);
        return Ok(vaccinations);
    }

    [HttpGet("{id}/prescriptions/active")]
    [ProducesResponseType<List<PrescriptionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get active prescriptions for pet")]
    [EndpointDescription("Returns all currently active prescriptions for the specified pet.")]
    public async Task<ActionResult<List<PrescriptionResponse>>> GetActivePrescriptions(int id, CancellationToken cancellationToken)
    {
        var prescriptions = await petService.GetActivePrescriptionsAsync(id, cancellationToken);
        return Ok(prescriptions);
    }
}
