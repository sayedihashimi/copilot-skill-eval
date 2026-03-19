using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext db) : IMedicalRecordService
{
    public async Task<MedicalRecordDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return record is null ? null : MapToDetailResponse(record);
    }

    public async Task<MedicalRecordDetailResponse> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
            ?? throw new InvalidOperationException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
        {
            throw new InvalidOperationException($"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");
        }

        if (await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct))
        {
            throw new InvalidOperationException($"A medical record already exists for appointment {request.AppointmentId}.");
        }

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

        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        await db.Entry(record).Reference(m => m.Pet).LoadAsync(ct);
        await db.Entry(record).Reference(m => m.Veterinarian).LoadAsync(ct);
        await db.Entry(record).Collection(m => m.Prescriptions).LoadAsync(ct);

        return MapToDetailResponse(record);
    }

    public async Task<MedicalRecordDetailResponse?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct = default)
    {
        var record = await db.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (record is null)
        {
            return null;
        }

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await db.SaveChangesAsync(ct);
        return MapToDetailResponse(record);
    }

    private static MedicalRecordDetailResponse MapToDetailResponse(MedicalRecord m) =>
        new(m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
            m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
            m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
            m.Prescriptions.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt)).ToList());
}
