using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.Id == id)
            .Select(m => new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId,
                m.Pet.Name, m.VeterinarianId,
                m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
                    p.CreatedAt)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([request.AppointmentId], ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new ArgumentException(
                $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var existingRecord = await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct);
        if (existingRecord)
            throw new InvalidOperationException("A medical record already exists for this appointment.");

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
            FollowUpDate = request.FollowUpDate
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Medical record created with ID {RecordId} for appointment {AppointmentId}",
            record.Id, record.AppointmentId);

        var pet = await db.Pets.AsNoTracking().FirstAsync(p => p.Id == record.PetId, ct);
        var vet = await db.Veterinarians.AsNoTracking().FirstAsync(v => v.Id == record.VeterinarianId, ct);

        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId,
            pet.Name, record.VeterinarianId,
            vet.FirstName + " " + vet.LastName,
            record.Diagnosis, record.Treatment, record.Notes,
            record.FollowUpDate, record.CreatedAt, []);
    }

    public async Task<MedicalRecordResponse> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Medical record {RecordId} updated", id);

        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId,
            record.Pet.Name, record.VeterinarianId,
            record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes,
            record.FollowUpDate, record.CreatedAt,
            record.Prescriptions.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
                p.CreatedAt)).ToList());
    }
}
