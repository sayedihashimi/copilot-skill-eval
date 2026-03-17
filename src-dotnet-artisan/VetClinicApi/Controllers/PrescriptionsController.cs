using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService = prescriptionService;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(int id, CancellationToken ct)
    {
        var prescription = await _prescriptionService.GetByIdAsync(id, ct);
        return prescription is not null ? Ok(prescription) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PrescriptionDto>> Create([FromBody] CreatePrescriptionRequest request, CancellationToken ct)
    {
        var prescription = await _prescriptionService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
