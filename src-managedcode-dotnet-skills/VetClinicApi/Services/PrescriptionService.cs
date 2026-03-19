using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext context, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.StartDate.AddDays(p.DurationDays),
                p.StartDate.AddDays(p.DurationDays) >= DateOnly.FromDateTime(DateTime.UtcNow),
                p.Instructions, p.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        if (!await context.MedicalRecords.AnyAsync(mr => mr.Id == request.MedicalRecordId, ct))
            throw new InvalidOperationException($"Medical record with ID {request.MedicalRecordId} not found.");

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            Instructions = request.Instructions,
        };

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created prescription {PrescriptionId} for record {RecordId}", prescription.Id, prescription.MedicalRecordId);

        return new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.IsActive, prescription.Instructions, prescription.CreatedAt);
    }
}
