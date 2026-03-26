using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct = default);
    Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(AppointmentDto? Appointment, string? Error)> CreateAsync(CreateAppointmentDto dto, CancellationToken ct = default);
    Task<(AppointmentDto? Appointment, string? Error)> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct = default);
}

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> s_activeStatuses =
        [AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn, AppointmentStatus.InProgress];

    public async Task<PaginatedResponse<AppointmentDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments
            .AsNoTracking()
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
            .Select(a => MapToDto(a))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
                .ThenInclude(m => m!.Prescriptions)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        var petDto = new PetDto(
            appointment.Pet.Id, appointment.Pet.Name, appointment.Pet.Species,
            appointment.Pet.Breed, appointment.Pet.DateOfBirth, appointment.Pet.Weight,
            appointment.Pet.Color, appointment.Pet.MicrochipNumber, appointment.Pet.IsActive,
            appointment.Pet.OwnerId, appointment.Pet.CreatedAt, appointment.Pet.UpdatedAt);

        var vetDto = new VeterinarianDto(
            appointment.Veterinarian.Id, appointment.Veterinarian.FirstName,
            appointment.Veterinarian.LastName, appointment.Veterinarian.Email,
            appointment.Veterinarian.Phone, appointment.Veterinarian.Specialization,
            appointment.Veterinarian.LicenseNumber, appointment.Veterinarian.IsAvailable,
            appointment.Veterinarian.HireDate);

        MedicalRecordDto? medicalRecordDto = null;
        if (appointment.MedicalRecord is not null)
        {
            var mr = appointment.MedicalRecord;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            medicalRecordDto = new MedicalRecordDto(
                mr.Id, mr.AppointmentId, mr.PetId, mr.VeterinarianId,
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt,
                mr.Prescriptions.Select(p => new PrescriptionDto(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                    p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= today, p.CreatedAt)).ToList());
        }

        return new AppointmentDetailDto(
            appointment.Id, appointment.PetId, petDto, appointment.VeterinarianId, vetDto,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, medicalRecordDto);
    }

    public async Task<(AppointmentDto? Appointment, string? Error)> CreateAsync(CreateAppointmentDto dto, CancellationToken ct = default)
    {
        if (dto.AppointmentDate <= DateTime.UtcNow)
        {
            return (null, "Appointment date must be in the future");
        }

        var petExists = await db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive, ct);
        if (!petExists)
        {
            return (null, "Pet not found or is inactive");
        }

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId, ct);
        if (!vetExists)
        {
            return (null, "Veterinarian not found");
        }

        var conflict = await HasSchedulingConflictAsync(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null, ct);
        if (conflict)
        {
            return (null, "Scheduling conflict: the veterinarian has an overlapping appointment");
        }

        var appointment = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId}", appointment.Id, appointment.PetId);
        return (MapToDto(appointment), null);
    }

    public async Task<(AppointmentDto? Appointment, string? Error)> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return (null, "Appointment not found");
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return (null, $"Cannot update appointment in {appointment.Status} status");
        }

        var conflict = await HasSchedulingConflictAsync(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id, ct);
        if (conflict)
        {
            return (null, "Scheduling conflict: the veterinarian has an overlapping appointment");
        }

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;

        await db.SaveChangesAsync(ct);

        // Reload navigation properties if changed
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Appointment updated: {AppointmentId}", appointment.Id);
        return (MapToDto(appointment), null);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null)
        {
            return (false, "Appointment not found");
        }

        var validTransition = IsValidTransition(appointment.Status, dto.Status);
        if (!validTransition)
        {
            return (false, $"Invalid status transition from {appointment.Status} to {dto.Status}");
        }

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
            {
                return (false, "Cancellation reason is required when cancelling an appointment");
            }

            if (appointment.AppointmentDate < DateTime.UtcNow)
            {
                return (false, "Cannot cancel a past appointment");
            }

            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = dto.Status;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", appointment.Id, appointment.Status);
        return (true, null);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToDto(a))
            .ToListAsync(ct);
    }

    private async Task<bool> HasSchedulingConflictAsync(int vetId, DateTime proposedStart, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var proposedEnd = proposedStart.AddMinutes(durationMinutes);

        var query = db.Appointments
            .Where(a => a.VeterinarianId == vetId && s_activeStatuses.Contains(a.Status));

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync(a =>
            proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            proposedEnd > a.AppointmentDate, ct);
    }

    private static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next) =>
        (current, next) switch
        {
            (AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn) => true,
            (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Scheduled, AppointmentStatus.NoShow) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.InProgress) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
            _ => false
        };

    private static AppointmentDto MapToDto(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
