using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> ActiveStatuses =
    [
        AppointmentStatus.Scheduled,
        AppointmentStatus.CheckedIn,
        AppointmentStatus.InProgress
    ];

    public async Task<PagedResult<AppointmentDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking().AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);
        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.VeterinarianId, a.AppointmentDate,
                a.DurationMinutes, a.Status.ToString(), a.Reason, a.Notes,
                a.CancellationReason, a.CreatedAt, a.UpdatedAt,
                new PetSummaryDto(a.Pet.Id, a.Pet.Name, a.Pet.Species, a.Pet.Breed, a.Pet.IsActive),
                new VeterinarianDto(a.Veterinarian.Id, a.Veterinarian.FirstName, a.Veterinarian.LastName,
                    a.Veterinarian.Email, a.Veterinarian.Phone, a.Veterinarian.Specialization,
                    a.Veterinarian.LicenseNumber, a.Veterinarian.IsAvailable, a.Veterinarian.HireDate),
                null))
            .ToListAsync(ct);

        return new PagedResult<AppointmentDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
                .ThenInclude(m => m!.Prescriptions)
            .Where(a => a.Id == id)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.VeterinarianId, a.AppointmentDate,
                a.DurationMinutes, a.Status.ToString(), a.Reason, a.Notes,
                a.CancellationReason, a.CreatedAt, a.UpdatedAt,
                new PetSummaryDto(a.Pet.Id, a.Pet.Name, a.Pet.Species, a.Pet.Breed, a.Pet.IsActive),
                new VeterinarianDto(a.Veterinarian.Id, a.Veterinarian.FirstName, a.Veterinarian.LastName,
                    a.Veterinarian.Email, a.Veterinarian.Phone, a.Veterinarian.Specialization,
                    a.Veterinarian.LicenseNumber, a.Veterinarian.IsAvailable, a.Veterinarian.HireDate),
                a.MedicalRecord == null ? null : new MedicalRecordDto(
                    a.MedicalRecord.Id, a.MedicalRecord.AppointmentId, a.MedicalRecord.PetId,
                    a.MedicalRecord.VeterinarianId, a.MedicalRecord.Diagnosis, a.MedicalRecord.Treatment,
                    a.MedicalRecord.Notes, a.MedicalRecord.FollowUpDate, a.MedicalRecord.CreatedAt,
                    a.MedicalRecord.Prescriptions.Select(p => new PrescriptionDto(
                        p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                        p.StartDate, p.EndDate, p.Instructions,
                        p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt)).ToList())))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId, ct))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId, ct))
            throw new BusinessRuleException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new BusinessRuleException("Appointment date must be in the future.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null, ct);

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

        logger.LogInformation("Appointment created: {AppointmentId} for pet {PetId} with vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return (await GetByIdAsync(appointment.Id, ct))!;
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId, ct))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId, ct))
            throw new BusinessRuleException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (appointment.VeterinarianId != dto.VeterinarianId || appointment.AppointmentDate != dto.AppointmentDate || appointment.DurationMinutes != dto.DurationMinutes)
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id, ct);

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        ValidateStatusTransition(appointment, dto);

        appointment.Status = dto.Status;

        if (dto.Status == AppointmentStatus.Cancelled)
            appointment.CancellationReason = dto.CancellationReason;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);
        return await GetByIdAsync(id, ct);
    }

    public async Task<List<AppointmentDto>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.VeterinarianId, a.AppointmentDate,
                a.DurationMinutes, a.Status.ToString(), a.Reason, a.Notes,
                a.CancellationReason, a.CreatedAt, a.UpdatedAt,
                new PetSummaryDto(a.Pet.Id, a.Pet.Name, a.Pet.Species, a.Pet.Breed, a.Pet.IsActive),
                new VeterinarianDto(a.Veterinarian.Id, a.Veterinarian.FirstName, a.Veterinarian.LastName,
                    a.Veterinarian.Email, a.Veterinarian.Phone, a.Veterinarian.Specialization,
                    a.Veterinarian.LicenseNumber, a.Veterinarian.IsAvailable, a.Veterinarian.HireDate),
                null))
            .ToListAsync(ct);
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        ActiveStatuses.Contains(a.Status));

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        var hasConflict = await query.AnyAsync(a =>
            appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            proposedEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new BusinessRuleException(
                "The veterinarian has a scheduling conflict for the requested time slot.",
                StatusCodes.Status409Conflict,
                "Scheduling Conflict");
    }

    private static void ValidateStatusTransition(Appointment appointment, UpdateAppointmentStatusDto dto)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
            [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
            [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
            [AppointmentStatus.Completed] = [],
            [AppointmentStatus.Cancelled] = [],
            [AppointmentStatus.NoShow] = []
        };

        if (!validTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new BusinessRuleException(
                $"Cannot transition from {appointment.Status} to {dto.Status}.");

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new BusinessRuleException("A cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new BusinessRuleException("Cannot cancel a past appointment.");
        }
    }
}
