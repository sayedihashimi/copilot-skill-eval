using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType<PrescriptionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get prescription by ID")]
    [EndpointDescription("Returns prescription details including computed end date and active status.")]
    public async Task<ActionResult<PrescriptionResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var prescription = await prescriptionService.GetByIdAsync(id, cancellationToken);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpPost]
    [ProducesResponseType<PrescriptionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Create a prescription")]
    [EndpointDescription("Creates a new prescription for a medical record.")]
    public async Task<ActionResult<PrescriptionResponse>> Create(
        [FromBody] CreatePrescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var prescription = await prescriptionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }
}
