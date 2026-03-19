using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstOrDefaultAsync(mr => mr.Id == id, ct);

        return record is null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct);
        if (appointment is null)
            throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new ArgumentException($"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var existingRecord = await db.MedicalRecords.AnyAsync(mr => mr.AppointmentId == request.AppointmentId, ct);
        if (existingRecord)
            throw new InvalidOperationException($"A medical record already exists for appointment {request.AppointmentId}.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate,
            CreatedAt = DateTime.UtcNow
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        await db.Entry(record).Reference(r => r.Pet).LoadAsync(ct);
        await db.Entry(record).Reference(r => r.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Medical record created with ID {RecordId} for appointment {AppointmentId}", record.Id, request.AppointmentId);
        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstOrDefaultAsync(mr => mr.Id == id, ct);

        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Medical record updated with ID {RecordId}", record.Id);
        return MapToResponse(record);
    }

    private static MedicalRecordResponse MapToResponse(MedicalRecord mr)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MedicalRecordResponse(
            mr.Id, mr.AppointmentId, mr.PetId, mr.Pet.Name,
            mr.VeterinarianId, $"{mr.Veterinarian.FirstName} {mr.Veterinarian.LastName}",
            mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate,
            mr.Prescriptions.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt)).ToList(),
            mr.CreatedAt);
    }
}
