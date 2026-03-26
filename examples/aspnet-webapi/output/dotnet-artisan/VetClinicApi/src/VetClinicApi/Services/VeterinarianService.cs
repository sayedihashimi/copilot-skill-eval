using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct = default);
    Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto, CancellationToken ct = default);
    Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct = default);
    Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct = default);
}

public sealed class VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Veterinarians.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(v => v.IsAvailable == isAvailable.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToDto(v))
            .ToListAsync(ct);

        return new PaginatedResponse<VeterinarianDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var vet = await db.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        return vet is null ? null : MapToDto(vet);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto, CancellationToken ct = default)
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

        db.Veterinarians.Add(vet);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Veterinarian created: {VetId} {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToDto(vet);
    }

    public async Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto, CancellationToken ct = default)
    {
        var vet = await db.Veterinarians.FindAsync([id], ct);
        if (vet is null)
        {
            return null;
        }

        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Veterinarian updated: {VetId}", vet.Id);
        return MapToDto(vet);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct = default)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static VeterinarianDto MapToDto(Veterinarian v) =>
        new(v.Id, v.FirstName, v.LastName, v.Email, v.Phone, v.Specialization, v.LicenseNumber, v.IsAvailable, v.HireDate);
}
