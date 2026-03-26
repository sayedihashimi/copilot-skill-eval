using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService : IVeterinarianService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<VeterinarianService> _logger;

    public VeterinarianService(VetClinicDbContext context, ILogger<VeterinarianService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination)
    {
        var query = _context.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.LastName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(v => MapToDto(v))
            .ToListAsync();

        return new PagedResult<VeterinarianDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<VeterinarianDto?> GetByIdAsync(int id)
    {
        var vet = await _context.Veterinarians.FindAsync(id);
        return vet == null ? null : MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto)
    {
        var vet = new Veterinarian
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            HireDate = dto.HireDate
        };

        _context.Veterinarians.Add(vet);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Veterinarian created: {VetId} {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto)
    {
        var vet = await _context.Veterinarians.FindAsync(id);
        if (vet == null) return null;

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Veterinarian updated: {VetId}", vet.Id);
        return MapToDto(vet);
    }

    public async Task<IEnumerable<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await _context.Appointments
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
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
            })
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination)
    {
        var query = _context.Appointments.Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AppointmentDto
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
            })
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

    private static VeterinarianDto MapToDto(Veterinarian vet) => new()
    {
        Id = vet.Id,
        FirstName = vet.FirstName,
        LastName = vet.LastName,
        Email = vet.Email,
        Phone = vet.Phone,
        Specialization = vet.Specialization,
        LicenseNumber = vet.LicenseNumber,
        IsAvailable = vet.IsAvailable,
        HireDate = vet.HireDate
    };
}
