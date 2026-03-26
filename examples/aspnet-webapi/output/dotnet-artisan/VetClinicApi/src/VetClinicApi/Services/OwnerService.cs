using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct = default);
    Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId, CancellationToken ct = default);
    Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct = default);
}

public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PaginatedResponse<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Owners.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(term) ||
                o.LastName.ToLower().Contains(term) ||
                o.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToDto(o))
            .ToListAsync(ct);

        return new PaginatedResponse<OwnerDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners
            .AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null)
        {
            return null;
        }

        return new OwnerDetailDto(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct = default)
    {
        var owner = new Owner
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Owner created: {OwnerId} {Name}", owner.Id, $"{owner.FirstName} {owner.LastName}");
        return MapToDto(owner);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct = default)
    {
        var owner = await db.Owners.FindAsync([id], ct);
        if (owner is null)
        {
            return null;
        }

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Owner updated: {OwnerId}", owner.Id);
        return MapToDto(owner);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null)
        {
            return (false, "Owner not found");
        }

        if (owner.Pets.Any(p => p.IsActive))
        {
            return (false, "Cannot delete owner with active pets");
        }

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Owner deleted: {OwnerId}", id);
        return (true, null);
    }

    public async Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId, CancellationToken ct = default)
    {
        return await db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetDto(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

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

    private static OwnerDto MapToDto(Owner o) =>
        new(o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
            o.Address, o.City, o.State, o.ZipCode, o.CreatedAt, o.UpdatedAt);
}
