using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db) : IAppointmentService
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> s_validTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = [],
    };

    public async Task<PagedResult<AppointmentResponse>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        if (vetId.HasValue)
        {
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        }

        if (petId.HasValue)
        {
            query = query.Where(a => a.PetId == petId.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return new PagedResult<AppointmentResponse>(items, totalCount, page, pageSize);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        var petDto = new PetResponse(
            appointment.Pet.Id, appointment.Pet.Name, appointment.Pet.Species, appointment.Pet.Breed,
            appointment.Pet.DateOfBirth, appointment.Pet.Weight, appointment.Pet.Color,
            appointment.Pet.MicrochipNumber, appointment.Pet.IsActive, appointment.Pet.OwnerId,
            appointment.Pet.CreatedAt, appointment.Pet.UpdatedAt);

        var vetDto = new VeterinarianResponse(
            appointment.Veterinarian.Id, appointment.Veterinarian.FirstName, appointment.Veterinarian.LastName,
            appointment.Veterinarian.Email, appointment.Veterinarian.Phone,
            appointment.Veterinarian.Specialization, appointment.Veterinarian.LicenseNumber,
            appointment.Veterinarian.IsAvailable, appointment.Veterinarian.HireDate);

        MedicalRecordResponse? medicalRecordDto = appointment.MedicalRecord is not null
            ? new MedicalRecordResponse(
                appointment.MedicalRecord.Id, appointment.MedicalRecord.AppointmentId,
                appointment.MedicalRecord.PetId, appointment.MedicalRecord.VeterinarianId,
                appointment.MedicalRecord.Diagnosis, appointment.MedicalRecord.Treatment,
                appointment.MedicalRecord.Notes, appointment.MedicalRecord.FollowUpDate,
                appointment.MedicalRecord.CreatedAt)
            : null;

        return new AppointmentDetailResponse(
            appointment.Id, appointment.PetId, petDto, appointment.VeterinarianId, vetDto,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status.ToString(),
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            medicalRecordDto, appointment.CreatedAt, appointment.UpdatedAt);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct = default)
    {
        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment date must be in the future.");
        }

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
        {
            throw new InvalidOperationException($"Active pet with ID {request.PetId} not found.");
        }

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
        {
            throw new InvalidOperationException($"Veterinarian with ID {request.VeterinarianId} not found.");
        }

        await CheckConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes,
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            throw new InvalidOperationException($"Cannot update an appointment with status '{appointment.Status}'.");
        }

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
        {
            throw new InvalidOperationException($"Active pet with ID {request.PetId} not found.");
        }

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
        {
            throw new InvalidOperationException($"Veterinarian with ID {request.VeterinarianId} not found.");
        }

        await CheckConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;

        await db.SaveChangesAsync(ct);

        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        if (!s_validTransitions.TryGetValue(appointment.Status, out var validTargets) || !validTargets.Contains(request.Status))
        {
            throw new InvalidOperationException($"Cannot transition from '{appointment.Status}' to '{request.Status}'.");
        }

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
            {
                throw new InvalidOperationException("A cancellation reason is required when cancelling an appointment.");
            }

            if (appointment.AppointmentDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Past appointments cannot be cancelled.");
            }

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        await db.SaveChangesAsync(ct);

        return MapToResponse(appointment);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);
    }

    private async Task CheckConflictAsync(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var appointmentEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeId.HasValue)
        {
            conflictQuery = conflictQuery.Where(a => a.Id != excludeId.Value);
        }

        var hasConflict = await conflictQuery
            .AnyAsync(a =>
                appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
                appointmentEnd > a.AppointmentDate, ct);

        if (hasConflict)
        {
            throw new InvalidOperationException("This veterinarian has a conflicting appointment at the requested time.");
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status.ToString(),
            a.Reason, a.Notes, a.CancellationReason, a.CreatedAt, a.UpdatedAt);
}
