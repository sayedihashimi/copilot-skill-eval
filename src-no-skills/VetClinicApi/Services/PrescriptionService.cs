using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
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

    public async Task<PrescriptionResponseDto> GetByIdAsync(int id)
    {
        var p = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Prescription with ID {id} not found.");
        return MapToResponse(p);
    }

    public async Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionDto dto)
    {
        if (!await _db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new KeyNotFoundException($"Medical record with ID {dto.MedicalRecordId} not found.");

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

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}", prescription.Id, prescription.MedicalRecordId);

        return MapToResponse(prescription);
    }

    private static PrescriptionResponseDto MapToResponse(Prescription p) => new()
    {
        Id = p.Id,
        MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName,
        Dosage = p.Dosage,
        DurationDays = p.DurationDays,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Instructions = p.Instructions,
        IsActive = p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
        CreatedAt = p.CreatedAt
    };
}
