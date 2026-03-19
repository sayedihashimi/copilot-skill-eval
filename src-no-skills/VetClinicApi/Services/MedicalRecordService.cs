using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MedicalRecordResponseDto> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId)
            ?? throw new KeyNotFoundException($"Appointment with ID {dto.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new BusinessRuleException("Medical records can only be created for appointments with status Completed or InProgress.");

        if (await _db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId))
            throw new BusinessRuleException("A medical record already exists for this appointment.", 409, "Conflict");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);

        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        return MapToResponse(record);
    }

    private static MedicalRecordResponseDto MapToResponse(MedicalRecord m) => new()
    {
        Id = m.Id,
        AppointmentId = m.AppointmentId,
        PetId = m.PetId,
        VeterinarianId = m.VeterinarianId,
        Diagnosis = m.Diagnosis,
        Treatment = m.Treatment,
        Notes = m.Notes,
        FollowUpDate = m.FollowUpDate,
        CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions?.Select(p => new PrescriptionResponseDto
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
        }).ToList() ?? new()
    };
}
