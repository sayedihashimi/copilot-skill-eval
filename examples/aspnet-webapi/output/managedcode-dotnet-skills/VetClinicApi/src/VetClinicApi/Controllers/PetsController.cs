using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/pets")]
[Produces("application/json")]
public class PetsController(IPetService petService) : ControllerBase
{
    /// <summary>List all active pets with optional search, species filter, and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await petService.GetAllAsync(search, species, includeInactive, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get a pet by ID including owner info.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Create a new pet.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto, CancellationToken ct)
    {
        var pet = await petService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    /// <summary>Update a pet (including owner transfer).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto, CancellationToken ct)
    {
        var pet = await petService.UpdateAsync(id, dto, ct);
        return pet is null ? NotFound() : Ok(pet);
    }

    /// <summary>Soft-delete a pet (set IsActive = false).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await petService.SoftDeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get all medical records for a pet.</summary>
    [HttpGet("{id:int}/medical-records")]
    [ProducesResponseType(typeof(List<MedicalRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMedicalRecords(int id, CancellationToken ct)
    {
        var records = await petService.GetMedicalRecordsAsync(id, ct);
        return Ok(records);
    }

    /// <summary>Get all vaccinations for a pet.</summary>
    [HttpGet("{id:int}/vaccinations")]
    [ProducesResponseType(typeof(List<VaccinationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVaccinations(int id, CancellationToken ct)
    {
        var vaccinations = await petService.GetVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    /// <summary>Get vaccinations that are due soon or overdue for a pet.</summary>
    [HttpGet("{id:int}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(List<VaccinationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id, CancellationToken ct)
    {
        var vaccinations = await petService.GetUpcomingVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    /// <summary>Get active prescriptions for a pet.</summary>
    [HttpGet("{id:int}/prescriptions/active")]
    [ProducesResponseType(typeof(List<PrescriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePrescriptions(int id, CancellationToken ct)
    {
        var prescriptions = await petService.GetActivePrescriptionsAsync(id, ct);
        return Ok(prescriptions);
    }
}
