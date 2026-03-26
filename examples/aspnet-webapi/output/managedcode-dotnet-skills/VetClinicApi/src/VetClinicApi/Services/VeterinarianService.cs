using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Veterinarians.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToDto(v))
            .ToListAsync(ct);

        return new PagedResult<VeterinarianDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vet = await db.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        return vet is null ? null : MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto, CancellationToken ct)
    {
        if (await db.Veterinarians.AnyAsync(v => v.Email == dto.Email, ct))
            throw new BusinessRuleException("A veterinarian with this email already exists.", StatusCodes.Status409Conflict, "Duplicate Email");

        if (await db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber, ct))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", StatusCodes.Status409Conflict, "Duplicate License");

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

        db.Veterinarians.Add(vet);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Veterinarian created: {VetId} Dr. {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto, CancellationToken ct)
    {
        var vet = await db.Veterinarians.FindAsync([id], ct);
        if (vet is null) return null;

        if (await db.Veterinarians.AnyAsync(v => v.Email == dto.Email && v.Id != id, ct))
            throw new BusinessRuleException("A veterinarian with this email already exists.", StatusCodes.Status409Conflict, "Duplicate Email");

        if (await db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber && v.Id != id, ct))
            throw new BusinessRuleException("A veterinarian with this license number already exists.", StatusCodes.Status409Conflict, "Duplicate License");

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await db.SaveChangesAsync(ct);
        return MapToDto(vet);
    }

    public async Task<List<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct)
    {
        if (!await db.Veterinarians.AnyAsync(v => v.Id == vetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await db.Appointments
            .AsNoTracking()
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.VeterinarianId, a.AppointmentDate,
                a.DurationMinutes, a.Status.ToString(), a.Reason, a.Notes,
                a.CancellationReason, a.CreatedAt, a.UpdatedAt,
                new PetSummaryDto(a.Pet.Id, a.Pet.Name, a.Pet.Species, a.Pet.Breed, a.Pet.IsActive),
                null, null))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Veterinarians.AnyAsync(v => v.Id == vetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var query = db.Appointments.AsNoTracking().Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

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
                null, null))
            .ToListAsync(ct);

        return new PagedResult<AppointmentDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private static VeterinarianDto MapToDto(Veterinarian v) => new(
        v.Id, v.FirstName, v.LastName, v.Email, v.Phone,
        v.Specialization, v.LicenseNumber, v.IsAvailable, v.HireDate);
}
