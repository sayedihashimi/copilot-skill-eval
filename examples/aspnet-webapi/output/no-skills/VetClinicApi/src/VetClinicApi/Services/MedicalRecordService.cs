using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
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

    public async Task<MedicalRecordResponseDto?> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        return record == null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> CreateAsync(MedicalRecordCreateDto dto)
    {
        var appointment = await _db.Appointments.FindAsync(dto.AppointmentId);
        if (appointment == null)
            throw new BusinessException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new BusinessException("Medical records can only be created for appointments with status 'Completed' or 'InProgress'.");

        if (await _db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId))
            throw new BusinessException("A medical record already exists for this appointment.");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto?> UpdateAsync(int id, MedicalRecordUpdateDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record == null) return null;

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        return MapToResponse(record);
    }

    internal static MedicalRecordResponseDto MapToResponse(MedicalRecord m)
    {
        return new MedicalRecordResponseDto
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
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList() ?? new()
        };
    }
}
