using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
[Produces("application/json")]
public class MedicalRecordsController(IMedicalRecordService recordService) : ControllerBase
{
    /// <summary>Get a medical record with prescriptions.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var record = await recordService.GetByIdAsync(id, ct);
        return record is null ? NotFound() : Ok(record);
    }

    /// <summary>Create a medical record (appointment must be Completed or InProgress).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto, CancellationToken ct)
    {
        var record = await recordService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    /// <summary>Update a medical record.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto, CancellationToken ct)
    {
        var record = await recordService.UpdateAsync(id, dto, ct);
        return record is null ? NotFound() : Ok(record);
    }
}
