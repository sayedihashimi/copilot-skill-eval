using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentResponseDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize)
    {
        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.AppointmentDate <= toDate.Value);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<AppointmentResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AppointmentResponseDto> GetByIdAsync(int id)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");
        return MapToResponse(appt);
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new BusinessRuleException("Appointment date must be in the future.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appt = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}", appt.Id, appt.PetId, appt.VeterinarianId);

        await _db.Entry(appt).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appt).Reference(a => a.Veterinarian).LoadAsync();
        return MapToResponse(appt);
    }

    public async Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        if (appt.Status == AppointmentStatus.Completed || appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.NoShow)
            throw new BusinessRuleException("Cannot update a completed, cancelled, or no-show appointment.");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate != appt.AppointmentDate || dto.VeterinarianId != appt.VeterinarianId || dto.DurationMinutes != appt.DurationMinutes)
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);

        appt.PetId = dto.PetId;
        appt.VeterinarianId = dto.VeterinarianId;
        appt.AppointmentDate = dto.AppointmentDate;
        appt.DurationMinutes = dto.DurationMinutes;
        appt.Reason = dto.Reason;
        appt.Notes = dto.Notes;
        appt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _db.Entry(appt).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appt).Reference(a => a.Veterinarian).LoadAsync();
        return MapToResponse(appt);
    }

    public async Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        ValidateStatusTransition(appt.Status, dto.Status);

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new BusinessRuleException("Cancellation reason is required when cancelling an appointment.");
            if (appt.AppointmentDate < DateTime.UtcNow)
                throw new BusinessRuleException("Cannot cancel a past appointment.");
            appt.CancellationReason = dto.CancellationReason;
        }

        appt.Status = dto.Status;
        appt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);
        return MapToResponse(appt);
    }

    public async Task<List<AppointmentResponseDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId)
    {
        var proposedStart = appointmentDate;
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = _db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        var hasConflict = await conflictQuery.AnyAsync(a =>
            proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) && proposedEnd > a.AppointmentDate);

        if (hasConflict)
            throw new BusinessRuleException("The veterinarian has a scheduling conflict for the requested time slot.", 409, "Scheduling Conflict");
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus target)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            { AppointmentStatus.Scheduled, new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow } },
            { AppointmentStatus.CheckedIn, new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled } },
            { AppointmentStatus.InProgress, new[] { AppointmentStatus.Completed } }
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
            throw new BusinessRuleException($"Cannot transition from {current} to {target}.");
    }

    private static AppointmentResponseDto MapToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        Pet = a.Pet != null ? new PetSummaryDto
        {
            Id = a.Pet.Id,
            Name = a.Pet.Name,
            Species = a.Pet.Species,
            Breed = a.Pet.Breed,
            IsActive = a.Pet.IsActive
        } : null,
        VeterinarianId = a.VeterinarianId,
        Veterinarian = a.Veterinarian != null ? new VeterinarianResponseDto
        {
            Id = a.Veterinarian.Id,
            FirstName = a.Veterinarian.FirstName,
            LastName = a.Veterinarian.LastName,
            Email = a.Veterinarian.Email,
            Phone = a.Veterinarian.Phone,
            Specialization = a.Veterinarian.Specialization,
            LicenseNumber = a.Veterinarian.LicenseNumber,
            IsAvailable = a.Veterinarian.IsAvailable,
            HireDate = a.Veterinarian.HireDate
        } : null,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        MedicalRecord = a.MedicalRecord != null ? new MedicalRecordSummaryDto
        {
            Id = a.MedicalRecord.Id,
            Diagnosis = a.MedicalRecord.Diagnosis,
            Treatment = a.MedicalRecord.Treatment,
            CreatedAt = a.MedicalRecord.CreatedAt
        } : null,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
