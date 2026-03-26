using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(MedicalRecordDto? Record, string? Error)> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct = default);
    Task<(MedicalRecordDto? Record, string? Error)> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct = default);
}

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt,
            record.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt)).ToList());
    }

    public async Task<(MedicalRecordDto? Record, string? Error)> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.FindAsync([dto.AppointmentId], ct);
        if (appointment is null)
        {
            return (null, "Appointment not found");
        }

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
        {
            return (null, "Medical record can only be created for appointments with status Completed or InProgress");
        }

        var existingRecord = await db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId, ct);
        if (existingRecord)
        {
            return (null, "A medical record already exists for this appointment");
        }

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

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        return (new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt, []), null);
    }

    public async Task<(MedicalRecordDto? Record, string? Error)> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null)
        {
            return (null, "Medical record not found");
        }

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Medical record updated: {RecordId}", record.Id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (new MedicalRecordDto(
            record.Id, record.AppointmentId, record.PetId, record.VeterinarianId,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt,
            record.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt)).ToList()), null);
    }
}
