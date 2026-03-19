using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService : IVeterinarianService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VeterinarianService> _logger;

    public VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize)
    {
        var query = _db.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.LastName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync();

        return new PagedResult<VeterinarianResponseDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<VeterinarianResponseDto> GetByIdAsync(int id)
    {
        var vet = await _db.Veterinarians.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Veterinarian with ID {id} not found.");
        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponseDto> CreateAsync(CreateVeterinarianDto dto)
    {
        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email))
            throw new BusinessRuleException("A veterinarian with this email already exists.", 409, "Conflict");
        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", 409, "Conflict");

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

        _db.Veterinarians.Add(vet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created veterinarian {VetId} ({Name})", vet.Id, $"{vet.FirstName} {vet.LastName}");

        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponseDto> UpdateAsync(int id, UpdateVeterinarianDto dto)
    {
        var vet = await _db.Veterinarians.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Veterinarian with ID {id} not found.");

        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email && v.Id != id))
            throw new BusinessRuleException("A veterinarian with this email already exists.", 409, "Conflict");
        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber && v.Id != id))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", 409, "Conflict");

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await _db.SaveChangesAsync();
        return MapToResponse(vet);
    }

    public async Task<List<AppointmentSummaryDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await _db.Appointments
            .Include(a => a.Pet)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Reason = a.Reason,
                DurationMinutes = a.DurationMinutes,
                PetName = a.Pet.Name,
                VetName = null
            }).ToListAsync();
    }

    public async Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var query = _db.Appointments
            .Include(a => a.Pet)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Reason = a.Reason,
                DurationMinutes = a.DurationMinutes,
                PetName = a.Pet.Name,
                VetName = null
            }).ToListAsync();

        return new PagedResult<AppointmentSummaryDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private static VeterinarianResponseDto MapToResponse(Veterinarian vet) => new()
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
