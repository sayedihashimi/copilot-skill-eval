using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Controllers;

[ApiController]
[Route("api/medical-records")]
public sealed class MedicalRecordsController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicalRecordDetailResponse>> GetById(int id, CancellationToken ct = default)
    {
        var record = await medicalRecordService.GetByIdAsync(id, ct);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    public async Task<ActionResult<MedicalRecordDetailResponse>> Create(CreateMedicalRecordRequest request, CancellationToken ct = default)
    {
        var record = await medicalRecordService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MedicalRecordDetailResponse>> Update(int id, UpdateMedicalRecordRequest request, CancellationToken ct = default)
    {
        var record = await medicalRecordService.UpdateAsync(id, request, ct);
        return record is null ? NotFound() : Ok(record);
    }
}
