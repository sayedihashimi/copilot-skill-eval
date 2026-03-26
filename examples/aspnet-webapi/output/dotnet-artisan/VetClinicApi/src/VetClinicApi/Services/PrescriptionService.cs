using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(PrescriptionDto? Prescription, string? Error)> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct = default);
}

public sealed class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prescription = await db.Prescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (prescription is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionDto(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.Instructions,
            prescription.EndDate >= today, prescription.CreatedAt);
    }

    public async Task<(PrescriptionDto? Prescription, string? Error)> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct = default)
    {
        var recordExists = await db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId, ct);
        if (!recordExists)
        {
            return (null, "Medical record not found");
        }

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddDays(dto.DurationDays),
            Instructions = dto.Instructions
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", prescription.Id, prescription.MedicalRecordId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (new PrescriptionDto(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.Instructions,
            prescription.EndDate >= today, prescription.CreatedAt), null);
    }
}
