using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Prescriptions)
            .Where(m => m.Id == id)
            .Select(m => new MedicalRecordDto(
                m.Id, m.AppointmentId, m.PetId, m.VeterinarianId,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionDto(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([dto.AppointmentId], ct)
            ?? throw new BusinessRuleException($"Appointment with ID {dto.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new BusinessRuleException("Medical records can only be created for appointments with status Completed or InProgress.");

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId, ct))
            throw new BusinessRuleException("A medical record already exists for this appointment.", StatusCodes.Status409Conflict, "Duplicate Record");

        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId, ct))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId, ct))
            throw new BusinessRuleException($"Veterinarian with ID {dto.VeterinarianId} not found.");

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

        logger.LogInformation("Medical record created: {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);
        return (await GetByIdAsync(record.Id, ct))!;
    }

    public async Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken ct)
    {
        var record = await db.MedicalRecords.FindAsync([id], ct);
        if (record is null) return null;

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }
}
