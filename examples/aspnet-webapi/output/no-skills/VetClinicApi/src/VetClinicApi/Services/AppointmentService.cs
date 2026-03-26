using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
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
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = _db.Appointments
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

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<AppointmentResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
                .ThenInclude(m => m!.Prescriptions)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new BusinessException("Active pet not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new BusinessException("Veterinarian not found.");
        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new BusinessException("Appointment date must be in the future.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appointment = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId}", appointment.Id, appointment.PetId);

        await _db.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto?> UpdateAsync(int id, AppointmentUpdateDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        if (appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.NoShow)
            throw new BusinessException($"Cannot update an appointment with status '{appointment.Status}'.");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new BusinessException("Active pet not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new BusinessException("Veterinarian not found.");

        if (appointment.AppointmentDate != dto.AppointmentDate ||
            appointment.VeterinarianId != dto.VeterinarianId ||
            appointment.DurationMinutes != dto.DurationMinutes)
        {
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);
        }

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto?> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        ValidateStatusTransition(appointment.Status, dto.Status);

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new BusinessException("Cancellation reason is required when cancelling an appointment.");
            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new BusinessException("Cannot cancel past appointments.");
            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = dto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);
        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponseDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId)
    {
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflicting = await _db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value))
            .ToListAsync();

        var hasConflict = conflicting.Any(a =>
        {
            var existingEnd = a.AppointmentDate.AddMinutes(a.DurationMinutes);
            return appointmentDate < existingEnd && proposedEnd > a.AppointmentDate;
        });

        if (hasConflict)
            throw new BusinessException("The veterinarian has a scheduling conflict at the requested time.");
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [AppointmentStatus.Scheduled] = new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            [AppointmentStatus.CheckedIn] = new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            [AppointmentStatus.InProgress] = new[] { AppointmentStatus.Completed },
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(next))
            throw new BusinessException($"Invalid status transition from '{current}' to '{next}'.");
    }

    internal static AppointmentResponseDto MapToResponse(Appointment a)
    {
        var dto = new AppointmentResponseDto
        {
            Id = a.Id,
            PetId = a.PetId,
            VeterinarianId = a.VeterinarianId,
            AppointmentDate = a.AppointmentDate,
            DurationMinutes = a.DurationMinutes,
            Status = a.Status.ToString(),
            Reason = a.Reason,
            Notes = a.Notes,
            CancellationReason = a.CancellationReason,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };

        if (a.Pet != null)
        {
            dto.Pet = new PetSummaryDto
            {
                Id = a.Pet.Id,
                Name = a.Pet.Name,
                Species = a.Pet.Species,
                Breed = a.Pet.Breed,
                IsActive = a.Pet.IsActive
            };
        }

        if (a.Veterinarian != null)
        {
            dto.Veterinarian = VeterinarianService.MapToResponse(a.Veterinarian);
        }

        if (a.MedicalRecord != null)
        {
            dto.MedicalRecord = MedicalRecordService.MapToResponse(a.MedicalRecord);
        }

        return dto;
    }
}
