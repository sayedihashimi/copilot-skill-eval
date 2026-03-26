using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = _context.Appointments.AsQueryable();

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
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id)
    {
        var appt = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return null;

        return new AppointmentDetailDto
        {
            Id = appt.Id,
            PetId = appt.PetId,
            VeterinarianId = appt.VeterinarianId,
            AppointmentDate = appt.AppointmentDate,
            DurationMinutes = appt.DurationMinutes,
            Status = appt.Status.ToString(),
            Reason = appt.Reason,
            Notes = appt.Notes,
            CancellationReason = appt.CancellationReason,
            CreatedAt = appt.CreatedAt,
            UpdatedAt = appt.UpdatedAt,
            Pet = new PetSummaryDto
            {
                Id = appt.Pet.Id,
                Name = appt.Pet.Name,
                Species = appt.Pet.Species,
                Breed = appt.Pet.Breed,
                IsActive = appt.Pet.IsActive
            },
            Veterinarian = new VeterinarianDto
            {
                Id = appt.Veterinarian.Id,
                FirstName = appt.Veterinarian.FirstName,
                LastName = appt.Veterinarian.LastName,
                Email = appt.Veterinarian.Email,
                Phone = appt.Veterinarian.Phone,
                Specialization = appt.Veterinarian.Specialization,
                LicenseNumber = appt.Veterinarian.LicenseNumber,
                IsAvailable = appt.Veterinarian.IsAvailable,
                HireDate = appt.Veterinarian.HireDate
            },
            MedicalRecord = appt.MedicalRecord == null ? null : new MedicalRecordDto
            {
                Id = appt.MedicalRecord.Id,
                AppointmentId = appt.MedicalRecord.AppointmentId,
                PetId = appt.MedicalRecord.PetId,
                VeterinarianId = appt.MedicalRecord.VeterinarianId,
                Diagnosis = appt.MedicalRecord.Diagnosis,
                Treatment = appt.MedicalRecord.Treatment,
                Notes = appt.MedicalRecord.Notes,
                FollowUpDate = appt.MedicalRecord.FollowUpDate,
                CreatedAt = appt.MedicalRecord.CreatedAt
            }
        };
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        // Validate pet exists
        var petExists = await _context.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive);
        if (!petExists) throw new InvalidOperationException("Active pet not found.");

        // Validate vet exists and is available
        var vet = await _context.Veterinarians.FindAsync(dto.VeterinarianId);
        if (vet == null) throw new InvalidOperationException("Veterinarian not found.");
        if (!vet.IsAvailable) throw new InvalidOperationException("Veterinarian is not available.");

        // Must be in the future
        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future.");

        // Check for scheduling conflicts
        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appointment = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}", appointment.Id, appointment.PetId, appointment.VeterinarianId);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return null;

        // Can only update non-terminal appointments
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update appointment in {appointment.Status} status.");

        // Validate pet exists
        var petExists = await _context.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive);
        if (!petExists) throw new InvalidOperationException("Active pet not found.");

        // Validate vet exists
        var vetExists = await _context.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId);
        if (!vetExists) throw new InvalidOperationException("Veterinarian not found.");

        // Check for scheduling conflicts if date/time/vet changed
        if (appointment.AppointmentDate != dto.AppointmentDate || appointment.VeterinarianId != dto.VeterinarianId || appointment.DurationMinutes != dto.DurationMinutes)
        {
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);
        }

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Appointment updated: {AppointmentId}", appointment.Id);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return null;

        if (!Enum.TryParse<AppointmentStatus>(dto.Status, true, out var newStatus))
            throw new InvalidOperationException($"Invalid status: {dto.Status}. Valid values: {string.Join(", ", Enum.GetNames<AppointmentStatus>())}");

        // Validate status transition
        ValidateStatusTransition(appointment.Status, newStatus);

        // Cancellation rules
        if (newStatus == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new InvalidOperationException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment.");

            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = newStatus;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", appointment.Id, newStatus);
        return MapToDto(appointment);
    }

    public async Task<IEnumerable<AppointmentDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Appointments
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime proposedStart, int durationMinutes, int? excludeAppointmentId)
    {
        var proposedEnd = proposedStart.AddMinutes(durationMinutes);

        var conflicting = await _context.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value)
                && a.AppointmentDate < proposedEnd
                && a.AppointmentDate.AddMinutes(a.DurationMinutes) > proposedStart)
            .AnyAsync();

        if (conflicting)
            throw new InvalidOperationException("Scheduling conflict: the veterinarian has an overlapping appointment during this time.");
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
            [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
            [AppointmentStatus.InProgress] = [AppointmentStatus.Completed]
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(next))
            throw new InvalidOperationException($"Invalid status transition from {current} to {next}.");
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
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
}
