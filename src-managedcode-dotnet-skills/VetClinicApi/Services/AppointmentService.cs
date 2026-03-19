using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext context, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> TerminalStatuses =
        [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow];

    public async Task<PagedResult<AppointmentResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = context.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return new PagedResult<AppointmentResponse>(items, totalCount, page, pageSize);
    }

    public async Task<AppointmentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.Id == id)
            .Select(a => MapToResponse(a))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (!await context.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new InvalidOperationException($"Active pet with ID {request.PetId} not found.");

        if (!await context.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId && v.IsAvailable, ct))
            throw new InvalidOperationException($"Available veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future.");

        // Check for scheduling conflicts
        var apptEnd = request.AppointmentDate.AddMinutes(request.DurationMinutes);
        var hasConflict = await context.Appointments
            .Where(a => a.VeterinarianId == request.VeterinarianId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow)
            .AnyAsync(a =>
                request.AppointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes)
                && apptEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException("This time slot conflicts with an existing appointment for this veterinarian.");

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes,
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync(ct);

        await context.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await context.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"Cannot update an appointment with status '{appointment.Status}'.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future.");

        // Check for scheduling conflicts (excluding this appointment)
        var apptEnd = request.AppointmentDate.AddMinutes(request.DurationMinutes);
        var hasConflict = await context.Appointments
            .Where(a => a.VeterinarianId == appointment.VeterinarianId
                && a.Id != id
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow)
            .AnyAsync(a =>
                request.AppointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes)
                && apptEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException("This time slot conflicts with an existing appointment for this veterinarian.");

        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Updated appointment {AppointmentId}", id);
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        ValidateStatusTransition(appointment, request);

        appointment.Status = request.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        if (request.Status == AppointmentStatus.Cancelled)
            appointment.CancellationReason = request.CancellationReason;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", id, request.Status);
        return MapToResponse(appointment);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);
    }

    private static void ValidateStatusTransition(Appointment appointment, UpdateAppointmentStatusRequest request)
    {
        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"Cannot change status from terminal state '{appointment.Status}'.");

        var validTransitions = appointment.Status switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>(),
        };

        if (!validTransitions.Contains(request.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{appointment.Status}' to '{request.Status}'. Valid transitions: {string.Join(", ", validTransitions)}.");

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new InvalidOperationException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment.");
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new(
        a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
        a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
        a.AppointmentDate, a.DurationMinutes, a.Status,
        a.Reason, a.Notes, a.CancellationReason,
        a.CreatedAt, a.UpdatedAt);
}
