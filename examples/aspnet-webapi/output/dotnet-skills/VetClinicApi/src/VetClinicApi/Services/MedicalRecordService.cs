using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext context, ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MedicalRecordDetailDto?> GetByIdAsync(int id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record == null) return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        return new MedicalRecordDetailDto
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            PetId = record.PetId,
            VeterinarianId = record.VeterinarianId,
            Diagnosis = record.Diagnosis,
            Treatment = record.Treatment,
            Notes = record.Notes,
            FollowUpDate = record.FollowUpDate,
            CreatedAt = record.CreatedAt,
            Prescriptions = record.Prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                MedicalRecordId = p.MedicalRecordId,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                DurationDays = p.DurationDays,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Instructions = p.Instructions,
                IsActive = p.EndDate >= today,
                CreatedAt = p.CreatedAt
            })
        };
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        // Validate appointment exists and has correct status
        var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
        if (appointment == null) throw new InvalidOperationException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Medical records can only be created for appointments with status Completed or InProgress.");

        // Check if a medical record already exists for this appointment
        var existing = await _context.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId);
        if (existing) throw new InvalidOperationException("A medical record already exists for this appointment.");

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

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        return new MedicalRecordDto
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            PetId = record.PetId,
            VeterinarianId = record.VeterinarianId,
            Diagnosis = record.Diagnosis,
            Treatment = record.Treatment,
            Notes = record.Notes,
            FollowUpDate = record.FollowUpDate,
            CreatedAt = record.CreatedAt
        };
    }

    public async Task<MedicalRecordDto?> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return null;

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Medical record updated: {RecordId}", record.Id);

        return new MedicalRecordDto
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            PetId = record.PetId,
            VeterinarianId = record.VeterinarianId,
            Diagnosis = record.Diagnosis,
            Treatment = record.Treatment,
            Notes = record.Notes,
            FollowUpDate = record.FollowUpDate,
            CreatedAt = record.CreatedAt
        };
    }
}
