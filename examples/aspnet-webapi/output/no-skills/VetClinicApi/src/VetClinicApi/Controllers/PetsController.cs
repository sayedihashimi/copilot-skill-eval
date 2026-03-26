using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service)
    {
        _service = service;
    }

    /// <summary>List all active pets with optional search, species filter, and pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PetResponseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? species,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, species, includeInactive, new PaginationParams { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>Get pet by ID including owner info</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var pet = await _service.GetByIdAsync(id);
        return pet == null ? NotFound() : Ok(pet);
    }

    /// <summary>Create a new pet</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponseDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] PetCreateDto dto)
    {
        var pet = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    /// <summary>Update pet (including owner transfer)</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PetResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] PetUpdateDto dto)
    {
        var pet = await _service.UpdateAsync(id, dto);
        return pet == null ? NotFound() : Ok(pet);
    }

    /// <summary>Soft-delete a pet (set IsActive = false)</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.SoftDeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    /// <summary>Get all medical records for a pet</summary>
    [HttpGet("{id}/medical-records")]
    [ProducesResponseType(typeof(List<MedicalRecordResponseDto>), 200)]
    public async Task<IActionResult> GetMedicalRecords(int id)
    {
        var records = await _service.GetMedicalRecordsAsync(id);
        return Ok(records);
    }

    /// <summary>Get all vaccinations for a pet</summary>
    [HttpGet("{id}/vaccinations")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    public async Task<IActionResult> GetVaccinations(int id)
    {
        var vaccinations = await _service.GetVaccinationsAsync(id);
        return Ok(vaccinations);
    }

    /// <summary>Get vaccinations that are due soon or overdue</summary>
    [HttpGet("{id}/vaccinations/upcoming")]
    [ProducesResponseType(typeof(List<VaccinationResponseDto>), 200)]
    public async Task<IActionResult> GetUpcomingVaccinations(int id)
    {
        var vaccinations = await _service.GetUpcomingVaccinationsAsync(id);
        return Ok(vaccinations);
    }

    /// <summary>Get active prescriptions for a pet</summary>
    [HttpGet("{id}/prescriptions/active")]
    [ProducesResponseType(typeof(List<PrescriptionResponseDto>), 200)]
    public async Task<IActionResult> GetActivePrescriptions(int id)
    {
        var prescriptions = await _service.GetActivePrescriptionsAsync(id);
        return Ok(prescriptions);
    }
}
