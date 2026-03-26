using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/prescriptions")]
[Produces("application/json")]
public class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    /// <summary>Get prescription details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var prescription = await prescriptionService.GetByIdAsync(id, ct);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    /// <summary>Create a prescription for a medical record.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto, CancellationToken ct)
    {
        var prescription = await prescriptionService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
