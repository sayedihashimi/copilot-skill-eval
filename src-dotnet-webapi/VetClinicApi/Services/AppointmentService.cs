using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public async Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);
        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
                .ThenInclude(mr => mr!.Prescriptions)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        MedicalRecordResponse? medicalRecord = null;
        if (appointment.MedicalRecord is not null)
        {
            var mr = appointment.MedicalRecord;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            medicalRecord = new MedicalRecordResponse(
                mr.Id, mr.AppointmentId, mr.PetId, appointment.Pet.Name,
                mr.VeterinarianId, $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}",
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate,
                mr.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= today, p.CreatedAt)).ToList(),
                mr.CreatedAt);
        }

        return new AppointmentDetailResponse(
            appointment.Id, appointment.PetId, appointment.Pet.Name,
            appointment.VeterinarianId, $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}",
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            medicalRecord, appointment.CreatedAt, appointment.UpdatedAt);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Appointment created with ID {AppointmentId}", appointment.Id);
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        if (appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.NoShow)
            throw new ArgumentException($"Cannot update an appointment with status '{appointment.Status}'.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate != appointment.AppointmentDate ||
            request.VeterinarianId != appointment.VeterinarianId ||
            request.DurationMinutes != appointment.DurationMinutes)
        {
            await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);
        }

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Appointment updated with ID {AppointmentId}", appointment.Id);
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        if (!ValidTransitions.TryGetValue(appointment.Status, out var allowedStatuses) ||
            !allowedStatuses.Contains(request.Status))
            throw new ArgumentException($"Cannot transition from '{appointment.Status}' to '{request.Status}'.");

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("CancellationReason is required when cancelling an appointment.");

            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new ArgumentException("Cannot cancel a past appointment.");

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", appointment.Id, request.Status);
        return MapToResponse(appointment);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        var hasConflict = await conflictQuery
            .AnyAsync(a =>
                appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
                proposedEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException("The veterinarian has a scheduling conflict at the requested time.");
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason, a.CreatedAt, a.UpdatedAt);
}
