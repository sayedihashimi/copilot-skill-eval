using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
public sealed class MedicalRecordsController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService = medicalRecordService;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicalRecordDto>> GetById(int id, CancellationToken ct)
    {
        var record = await _medicalRecordService.GetByIdAsync(id, ct);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await _medicalRecordService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MedicalRecordDto>> Update(int id, [FromBody] UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await _medicalRecordService.UpdateAsync(id, request, ct);
        return record is not null ? Ok(record) : NotFound();
    }
}
