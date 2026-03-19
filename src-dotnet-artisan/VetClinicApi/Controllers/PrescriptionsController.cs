using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrescriptionResponse>> GetById(int id, CancellationToken ct = default)
    {
        var prescription = await prescriptionService.GetByIdAsync(id, ct);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpPost]
    public async Task<ActionResult<PrescriptionResponse>> Create(CreatePrescriptionRequest request, CancellationToken ct = default)
    {
        var prescription = await prescriptionService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
