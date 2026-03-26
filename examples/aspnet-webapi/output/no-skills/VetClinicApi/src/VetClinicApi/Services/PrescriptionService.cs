using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto?> GetByIdAsync(int id)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        return prescription == null ? null : MapToResponse(prescription);
    }

    public async Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto dto)
    {
        if (!await _db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new BusinessException("Medical record not found.");

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddDays(dto.DurationDays),
            Instructions = dto.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", prescription.Id, prescription.MedicalRecordId);

        return MapToResponse(prescription);
    }

    private static PrescriptionResponseDto MapToResponse(Prescription p)
    {
        return new PrescriptionResponseDto
        {
            Id = p.Id,
            MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            DurationDays = p.DurationDays,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Instructions = p.Instructions,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }
}
