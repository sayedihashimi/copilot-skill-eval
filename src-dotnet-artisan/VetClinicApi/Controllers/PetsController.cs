using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PetsController(IPetService petService) : ControllerBase
{
    private readonly IPetService _petService = petService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PetDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _petService.GetAllAsync(search, species, includeInactive, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PetDetailDto>> GetById(int id, CancellationToken ct)
    {
        var pet = await _petService.GetByIdAsync(id, ct);
        return pet is not null ? Ok(pet) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PetDto>> Create([FromBody] CreatePetRequest request, CancellationToken ct)
    {
        var pet = await _petService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PetDto>> Update(int id, [FromBody] UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await _petService.UpdateAsync(id, request, ct);
        return pet is not null ? Ok(pet) : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _petService.SoftDeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/medical-records")]
    public async Task<ActionResult<IReadOnlyList<MedicalRecordDto>>> GetMedicalRecords(int id, CancellationToken ct)
    {
        var records = await _petService.GetMedicalRecordsAsync(id, ct);
        return Ok(records);
    }

    [HttpGet("{id:int}/vaccinations")]
    public async Task<ActionResult<IReadOnlyList<VaccinationDto>>> GetVaccinations(int id, CancellationToken ct)
    {
        var vaccinations = await _petService.GetVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    [HttpGet("{id:int}/vaccinations/upcoming")]
    public async Task<ActionResult<IReadOnlyList<VaccinationDto>>> GetUpcomingVaccinations(int id, CancellationToken ct)
    {
        var vaccinations = await _petService.GetUpcomingVaccinationsAsync(id, ct);
        return Ok(vaccinations);
    }

    [HttpGet("{id:int}/prescriptions/active")]
    public async Task<ActionResult<IReadOnlyList<PrescriptionDto>>> GetActivePrescriptions(int id, CancellationToken ct)
    {
        var prescriptions = await _petService.GetActivePrescriptionsAsync(id, ct);
        return Ok(prescriptions);
    }
}
