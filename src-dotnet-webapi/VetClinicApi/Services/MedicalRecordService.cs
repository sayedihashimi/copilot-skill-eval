using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService(VetClinicDbContext db) : IMedicalRecordService
{
    public async Task<MedicalRecordResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var record = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return record is null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct);

        if (appointment is null)
            throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new ArgumentException($"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var existingRecord = await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct);
        if (existingRecord)
            throw new InvalidOperationException($"A medical record already exists for appointment ID {request.AppointmentId}.");

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate,
            CreatedAt = DateTime.UtcNow
        };

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        record.Pet = appointment.Pet;
        record.Veterinarian = appointment.Veterinarian;

        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);
        return MapToResponse(record);
    }

    private static MedicalRecordResponse MapToResponse(MedicalRecord m) => new()
    {
        Id = m.Id,
        AppointmentId = m.AppointmentId,
        PetId = m.PetId,
        Pet = m.Pet is null ? null : new PetSummaryResponse { Id = m.Pet.Id, Name = m.Pet.Name, Species = m.Pet.Species, Breed = m.Pet.Breed, IsActive = m.Pet.IsActive },
        VeterinarianId = m.VeterinarianId,
        Veterinarian = m.Veterinarian is null ? null : new VeterinarianSummaryResponse { Id = m.Veterinarian.Id, FirstName = m.Veterinarian.FirstName, LastName = m.Veterinarian.LastName, Specialization = m.Veterinarian.Specialization },
        Diagnosis = m.Diagnosis,
        Treatment = m.Treatment,
        Notes = m.Notes,
        FollowUpDate = m.FollowUpDate,
        CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions.Select(p => new PrescriptionResponse
        {
            Id = p.Id,
            MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            DurationDays = p.DurationDays,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            Instructions = p.Instructions,
            CreatedAt = p.CreatedAt
        }).ToList()
    };
}
