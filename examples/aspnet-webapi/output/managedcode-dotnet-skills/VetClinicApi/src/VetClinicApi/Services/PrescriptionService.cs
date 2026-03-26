using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId, ct))
            throw new BusinessRuleException($"Medical record with ID {dto.MedicalRecordId} not found.");

        var endDate = dto.StartDate.AddDays(dto.DurationDays);

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = endDate,
            Instructions = dto.Instructions
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Prescription created: {PrescriptionId} for medical record {RecordId}", prescription.Id, prescription.MedicalRecordId);
        return (await GetByIdAsync(prescription.Id, ct))!;
    }
}
