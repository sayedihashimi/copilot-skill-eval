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
        DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        query = query.OrderByDescending(a => a.AppointmentDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt, null))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<AppointmentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        MedicalRecordSummaryResponse? mrSummary = appointment.MedicalRecord is not null
            ? new MedicalRecordSummaryResponse(
                appointment.MedicalRecord.Id,
                appointment.MedicalRecord.Diagnosis,
                appointment.MedicalRecord.Treatment)
            : null;

        return new AppointmentResponse(
            appointment.Id, appointment.PetId, appointment.Pet.Name,
            appointment.VeterinarianId,
            appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, mrSummary);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckForConflicts(request.VeterinarianId, request.AppointmentDate,
            request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            Notes = request.Notes
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment created with ID {AppointmentId}", appointment.Id);

        var pet = await db.Pets.AsNoTracking().FirstAsync(p => p.Id == appointment.PetId, ct);
        var vet = await db.Veterinarians.AsNoTracking().FirstAsync(v => v.Id == appointment.VeterinarianId, ct);

        return new AppointmentResponse(
            appointment.Id, appointment.PetId, pet.Name,
            appointment.VeterinarianId, vet.FirstName + " " + vet.LastName,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, null);
    }

    public async Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update an appointment with status '{appointment.Status}'.");

        await CheckForConflicts(request.VeterinarianId, request.AppointmentDate,
            request.DurationMinutes, id, ct);

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment {AppointmentId} updated", id);

        var pet = await db.Pets.AsNoTracking().FirstAsync(p => p.Id == appointment.PetId, ct);
        var vet = await db.Veterinarians.AsNoTracking().FirstAsync(v => v.Id == appointment.VeterinarianId, ct);

        return new AppointmentResponse(
            appointment.Id, appointment.PetId, pet.Name,
            appointment.VeterinarianId, vet.FirstName + " " + vet.LastName,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, null);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        if (!ValidTransitions.TryGetValue(appointment.Status, out var allowedStatuses) ||
            !allowedStatuses.Contains(request.Status))
        {
            var validOptions = allowedStatuses is not null
                ? string.Join(", ", allowedStatuses)
                : "none";
            throw new ArgumentException(
                $"Cannot transition from '{appointment.Status}' to '{request.Status}'. " +
                $"Valid transitions: {validOptions}.");
        }

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("CancellationReason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new ArgumentException("Cannot cancel an appointment that is in the past.");

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, request.Status);

        return new AppointmentResponse(
            appointment.Id, appointment.PetId, appointment.Pet.Name,
            appointment.VeterinarianId,
            appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, null);
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
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt, null))
            .ToListAsync(ct);
    }

    private async Task CheckForConflicts(
        int vetId, DateTime appointmentDate, int durationMinutes,
        int? excludeAppointmentId, CancellationToken ct)
    {
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        var hasConflict = await conflictQuery.AnyAsync(a =>
            appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            proposedEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException(
                "The veterinarian has a scheduling conflict for the requested time slot.");
    }
}
