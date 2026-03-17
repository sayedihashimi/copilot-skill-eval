using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext context) : IPrescriptionService
{
    private readonly VetClinicDbContext _context = context;

    public async Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct)
    {
        var recordExists = await _context.MedicalRecords.AsNoTracking()
            .AnyAsync(m => m.Id == request.MedicalRecordId, ct);
        if (!recordExists)
            throw new KeyNotFoundException($"Medical record with ID {request.MedicalRecordId} not found.");

        var endDate = request.StartDate.AddDays(request.DurationDays);

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            EndDate = endDate,
            Instructions = request.Instructions
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionDto(
            prescription.Id, prescription.MedicalRecordId,
            prescription.MedicationName, prescription.Dosage,
            prescription.DurationDays, prescription.StartDate, prescription.EndDate,
            prescription.Instructions, prescription.EndDate >= today,
            prescription.CreatedAt);
    }
}
