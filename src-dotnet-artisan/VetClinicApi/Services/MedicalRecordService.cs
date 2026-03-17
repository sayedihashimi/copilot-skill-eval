using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class MedicalRecordService(VetClinicDbContext context) : IMedicalRecordService
{
    private readonly VetClinicDbContext _context = context;

    public async Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.Id == id)
            .Select(m => MapToDto(m))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordRequest request, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct);

        if (appointment is null)
            throw new KeyNotFoundException($"Appointment with ID {request.AppointmentId} not found.");

        if (appointment.Status is not (AppointmentStatus.Completed or AppointmentStatus.InProgress))
            throw new InvalidOperationException(
                $"Medical records can only be created for appointments with status 'Completed' or 'InProgress'. Current status: '{appointment.Status}'.");

        var existingRecord = await _context.MedicalRecords.AsNoTracking()
            .AnyAsync(m => m.AppointmentId == request.AppointmentId, ct);
        if (existingRecord)
            throw new InvalidOperationException(
                $"A medical record already exists for appointment {request.AppointmentId}.");

        var record = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync(ct);

        var created = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == record.Id, ct);

        return MapToDto(created);
    }

    public async Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordRequest request, CancellationToken ct)
    {
        var record = await _context.MedicalRecords.FindAsync([id], ct);
        if (record is null) return null;

        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;

        await _context.SaveChangesAsync(ct);

        var updated = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .FirstAsync(m => m.Id == id, ct);

        return MapToDto(updated);
    }

    private static MedicalRecordDto MapToDto(MedicalRecord m) =>
        new(m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
            m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
            m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
            m.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt)).ToList());
}
