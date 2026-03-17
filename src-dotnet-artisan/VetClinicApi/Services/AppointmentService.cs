using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext context) : IAppointmentService
{
    private readonly VetClinicDbContext _context = context;

    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public async Task<PaginatedResponse<AppointmentDto>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Appointments
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

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToDto(a))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentDto>(items, page, pageSize, totalCount);
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Pet)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Veterinarian)
            .Where(a => a.Id == id)
            .Select(a => new AppointmentDetailDto(
                a.Id, a.PetId, a.Pet.Name, a.Pet.Species,
                a.Pet.OwnerId, $"{a.Pet.Owner.FirstName} {a.Pet.Owner.LastName}",
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.MedicalRecord != null
                    ? new MedicalRecordDto(
                        a.MedicalRecord.Id, a.MedicalRecord.AppointmentId,
                        a.MedicalRecord.PetId, a.MedicalRecord.Pet.Name,
                        a.MedicalRecord.VeterinarianId,
                        $"{a.MedicalRecord.Veterinarian.FirstName} {a.MedicalRecord.Veterinarian.LastName}",
                        a.MedicalRecord.Diagnosis, a.MedicalRecord.Treatment,
                        a.MedicalRecord.Notes, a.MedicalRecord.FollowUpDate,
                        a.MedicalRecord.CreatedAt,
                        a.MedicalRecord.Prescriptions.Select(p => new PrescriptionDto(
                            p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                            p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                            p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
                            p.CreatedAt)).ToList())
                    : null,
                a.CreatedAt, a.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        var petExists = await _context.Pets.AsNoTracking()
            .AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate,
            request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appointment.Id, ct);

        return MapToDto(created);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await _context.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update an appointment with status '{appointment.Status}'.");

        var petExists = await _context.Pets.AsNoTracking()
            .AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (appointment.VeterinarianId != request.VeterinarianId ||
            appointment.AppointmentDate != request.AppointmentDate ||
            appointment.DurationMinutes != request.DurationMinutes)
        {
            await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate,
                request.DurationMinutes, id, ct);
        }

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var updated = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == id, ct);

        return MapToDto(updated);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        if (!ValidTransitions.TryGetValue(appointment.Status, out var validNext) ||
            !validNext.Contains(request.Status))
        {
            throw new InvalidOperationException(
                $"Cannot transition from '{appointment.Status}' to '{request.Status}'.");
        }

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment.");

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return MapToDto(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToDto(a))
            .ToListAsync(ct);
    }

    private async Task CheckSchedulingConflict(
        int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var appointmentEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        var hasConflict = await query.AnyAsync(a =>
            appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            appointmentEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException(
                "The veterinarian has a scheduling conflict at the requested time.");
    }

    private static AppointmentDto MapToDto(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name,
            a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
