using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PetsController(IPetService petService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PetResponse>>> GetAll(
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PetDetailResponse>> GetById(int id, CancellationToken ct = default)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    public async Task<ActionResult<PetResponse>> Create(CreatePetRequest request, CancellationToken ct = default)
    {
        var pet = await petService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PetResponse>> Update(int id, UpdatePetRequest request, CancellationToken ct = default)
    {
        var pet = await petService.UpdateAsync(id, request, ct);
        return pet is null ? NotFound() : Ok(pet);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var deleted = await petService.SoftDeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/medical-records")]
    public async Task<ActionResult<IReadOnlyList<MedicalRecordResponse>>> GetMedicalRecords(int id, CancellationToken ct = default)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        if (pet is null)
        {
            return NotFound();
        }

        var records = await petService.GetMedicalRecordsAsync(id, ct);
        return Ok(records);
    }

    [HttpGet("{id:int}/vaccinations")]
    public async Task<ActionResult<IReadOnlyList<VaccinationResponse>>> GetVaccinations(int id, CancellationToken ct = default)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        if (pet is null)
        {
            return NotFound();
        }

        var vaccinations = await petService.GetVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    [HttpGet("{id:int}/vaccinations/upcoming")]
    public async Task<ActionResult<IReadOnlyList<VaccinationResponse>>> GetUpcomingVaccinations(int id, CancellationToken ct = default)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        if (pet is null)
        {
            return NotFound();
        }

        var vaccinations = await petService.GetUpcomingVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    [HttpGet("{id:int}/prescriptions/active")]
    public async Task<ActionResult<IReadOnlyList<PrescriptionResponse>>> GetActivePrescriptions(int id, CancellationToken ct = default)
    {
        var pet = await petService.GetByIdAsync(id, ct);
        if (pet is null)
        {
            return NotFound();
        }

        var prescriptions = await petService.GetActivePrescriptionsAsync(id, ct);
        return Ok(prescriptions);
    }
}
