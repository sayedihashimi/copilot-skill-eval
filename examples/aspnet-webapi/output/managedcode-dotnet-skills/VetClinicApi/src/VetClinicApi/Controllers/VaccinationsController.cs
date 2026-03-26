using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/vaccinations")]
[Produces("application/json")]
public class VaccinationsController(IVaccinationService vaccinationService) : ControllerBase
{
    /// <summary>Get vaccination details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VaccinationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var vaccination = await vaccinationService.GetByIdAsync(id, ct);
        return vaccination is null ? NotFound() : Ok(vaccination);
    }

    /// <summary>Record a new vaccination.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VaccinationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVaccinationDto dto, CancellationToken ct)
    {
        var vaccination = await vaccinationService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }
}
