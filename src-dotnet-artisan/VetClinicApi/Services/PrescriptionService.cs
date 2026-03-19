using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext db) : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prescription = await db.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return prescription is null ? null : MapToResponse(prescription);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct = default)
    {
        if (!await db.MedicalRecords.AnyAsync(m => m.Id == request.MedicalRecordId, ct))
        {
            throw new InvalidOperationException($"Medical record with ID {request.MedicalRecordId} not found.");
        }

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            Instructions = request.Instructions,
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);
        return MapToResponse(prescription);
    }

    private static PrescriptionResponse MapToResponse(Prescription p) =>
        new(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
            p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
            p.Instructions, p.CreatedAt);
}
