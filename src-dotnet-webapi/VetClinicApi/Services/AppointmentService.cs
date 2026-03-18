using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService(VetClinicDbContext db) : IAppointmentService
{
    // Valid status transitions
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public async Task<PagedResponse<AppointmentResponse>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
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
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>
        {
            Items = items.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AppointmentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return appointment is null ? null : MapToResponse(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId && v.IsAvailable, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Available veterinarian with ID {request.VeterinarianId} not found.");

        await CheckConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

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
            throw new InvalidOperationException($"Cannot update an appointment with status '{appointment.Status}'.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId && v.IsAvailable, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Available veterinarian with ID {request.VeterinarianId} not found.");

        await CheckConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);

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

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        // Validate status transition
        if (!ValidTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(request.Status))
            throw new ArgumentException($"Cannot transition from '{appointment.Status}' to '{request.Status}'.");

        // Cancellation rules
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
        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var appointments = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(ct);

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var newEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeId.Value);

        // Pull potential conflicts into memory for precise overlap check
        var potentialConflicts = await conflictQuery.ToListAsync(ct);

        var hasConflict = potentialConflicts.Any(existing =>
        {
            var existingEnd = existing.AppointmentDate.AddMinutes(existing.DurationMinutes);
            return appointmentDate < existingEnd && newEnd > existing.AppointmentDate;
        });

        if (hasConflict)
            throw new InvalidOperationException("The veterinarian has a scheduling conflict for the requested time slot.");
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        Pet = a.Pet is null ? null : new PetSummaryResponse { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
        VeterinarianId = a.VeterinarianId,
        Veterinarian = a.Veterinarian is null ? null : new VeterinarianSummaryResponse { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization },
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        MedicalRecord = a.MedicalRecord is null ? null : new MedicalRecordSummaryResponse
        {
            Id = a.MedicalRecord.Id,
            Diagnosis = a.MedicalRecord.Diagnosis,
            Treatment = a.MedicalRecord.Treatment,
            CreatedAt = a.MedicalRecord.CreatedAt
        }
    };
}
