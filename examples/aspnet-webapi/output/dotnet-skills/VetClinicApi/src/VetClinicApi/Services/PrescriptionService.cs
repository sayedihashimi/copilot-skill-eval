using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext context, ILogger<PrescriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PrescriptionDto?> GetByIdAsync(int id)
    {
        var rx = await _context.Prescriptions.FindAsync(id);
        if (rx == null) return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        return new PrescriptionDto
        {
            Id = rx.Id,
            MedicalRecordId = rx.MedicalRecordId,
            MedicationName = rx.MedicationName,
            Dosage = rx.Dosage,
            DurationDays = rx.DurationDays,
            StartDate = rx.StartDate,
            EndDate = rx.EndDate,
            Instructions = rx.Instructions,
            IsActive = rx.EndDate >= today,
            CreatedAt = rx.CreatedAt
        };
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto)
    {
        var recordExists = await _context.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId);
        if (!recordExists) throw new InvalidOperationException("Medical record not found.");

        var endDate = dto.StartDate.AddDays(dto.DurationDays);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var rx = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = endDate,
            Instructions = dto.Instructions
        };

        _context.Prescriptions.Add(rx);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", rx.Id, rx.MedicalRecordId);

        return new PrescriptionDto
        {
            Id = rx.Id,
            MedicalRecordId = rx.MedicalRecordId,
            MedicationName = rx.MedicationName,
            Dosage = rx.Dosage,
            DurationDays = rx.DurationDays,
            StartDate = rx.StartDate,
            EndDate = rx.EndDate,
            Instructions = rx.Instructions,
            IsActive = rx.EndDate >= today,
            CreatedAt = rx.CreatedAt
        };
    }
}
