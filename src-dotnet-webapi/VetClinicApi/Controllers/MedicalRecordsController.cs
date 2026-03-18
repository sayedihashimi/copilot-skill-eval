using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
public class MedicalRecordsController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType<MedicalRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get medical record by ID")]
    [EndpointDescription("Returns medical record details including prescriptions.")]
    public async Task<ActionResult<MedicalRecordResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var record = await medicalRecordService.GetByIdAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    [ProducesResponseType<MedicalRecordResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a medical record")]
    [EndpointDescription("Creates a medical record for a completed or in-progress appointment. Only one record per appointment.")]
    public async Task<ActionResult<MedicalRecordResponse>> Create(
        [FromBody] CreateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        var record = await medicalRecordService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<MedicalRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a medical record")]
    [EndpointDescription("Updates medical record diagnosis, treatment, notes, and follow-up date.")]
    public async Task<ActionResult<MedicalRecordResponse>> Update(
        int id,
        [FromBody] UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        var record = await medicalRecordService.UpdateAsync(id, request, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }
}
