using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext context, ILogger<MedicalRecordService> logger) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.MedicalRecords
            .AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .Where(mr => mr.Id == id)
            .Select(mr => new MedicalRecordResponse(
                mr.Id, mr.AppointmentId, mr.PetId, mr.Pet.Name,
                mr.VeterinarianId, mr.Veterinarian.FirstName + " " + mr.Veterinarian.LastName,
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt,
                mr.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.StartDate.AddDays(p.DurationDays),
                    p.StartDate.AddDays(p.DurationDays) >= DateOnly.FromDateTime(DateTime.UtcNow),
                    p.Instructions, p.CreatedAt)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new InvalidOperationException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Medical records can only be created for Completed or InProgress appointments.");

        if (await context.MedicalRecords.AnyAsync(mr => mr.AppointmentId == request.AppointmentId, ct))
            throw new InvalidOperationException("A medical record already exists for this appointment.");

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate,
        };

        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created medical record {RecordId} for appointment {AppointmentId}", record.Id, record.AppointmentId);

        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, appointment.Pet.Name,
            record.VeterinarianId, appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt,
            []);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await context.MedicalRecords
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .FirstOrDefaultAsync(mr => mr.Id == id, ct);

        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Updated medical record {RecordId}", id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MedicalRecordResponse(
            record.Id, record.AppointmentId, record.PetId, record.Pet.Name,
            record.VeterinarianId, record.Veterinarian.FirstName + " " + record.Veterinarian.LastName,
            record.Diagnosis, record.Treatment, record.Notes, record.FollowUpDate, record.CreatedAt,
            record.Prescriptions.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.StartDate.AddDays(p.DurationDays),
                p.StartDate.AddDays(p.DurationDays) >= today,
                p.Instructions, p.CreatedAt)).ToList());
    }
}
