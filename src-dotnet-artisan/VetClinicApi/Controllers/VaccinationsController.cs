using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VaccinationsController(IVaccinationService vaccinationService) : ControllerBase
{
    private readonly IVaccinationService _vaccinationService = vaccinationService;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VaccinationDto>> GetById(int id, CancellationToken ct)
    {
        var vaccination = await _vaccinationService.GetByIdAsync(id, ct);
        return vaccination is not null ? Ok(vaccination) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<VaccinationDto>> Create([FromBody] CreateVaccinationRequest request, CancellationToken ct)
    {
        var vaccination = await _vaccinationService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }
}
