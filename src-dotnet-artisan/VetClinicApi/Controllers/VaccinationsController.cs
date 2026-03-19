using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VaccinationsController(IVaccinationService vaccinationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VaccinationResponse>> Create(CreateVaccinationRequest request, CancellationToken ct = default)
    {
        var vaccination = await vaccinationService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VaccinationResponse>> GetById(int id, CancellationToken ct = default)
    {
        var vaccination = await vaccinationService.GetByIdAsync(id, ct);
        return vaccination is null ? NotFound() : Ok(vaccination);
    }
}
