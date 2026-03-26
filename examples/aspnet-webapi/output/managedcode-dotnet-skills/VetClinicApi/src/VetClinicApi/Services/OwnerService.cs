using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
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
            .Select(o => MapToDto(o, false))
            .ToListAsync(ct);

        return new PagedResult<OwnerDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<OwnerDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners
            .AsNoTracking()
            .Include(o => o.Pets.Where(p => p.IsActive))
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return owner is null ? null : MapToDto(owner, true);
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto, CancellationToken ct)
    {
        if (await db.Owners.AnyAsync(o => o.Email == dto.Email, ct))
            throw new BusinessRuleException("An owner with this email already exists.", StatusCodes.Status409Conflict, "Duplicate Email");

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
        return MapToDto(owner, false);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto, CancellationToken ct)
    {
        var owner = await db.Owners.FindAsync([id], ct);
        if (owner is null) return null;

        if (await db.Owners.AnyAsync(o => o.Email == dto.Email && o.Id != id, ct))
            throw new BusinessRuleException("An owner with this email already exists.", StatusCodes.Status409Conflict, "Duplicate Email");

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;

        await db.SaveChangesAsync(ct);
        return MapToDto(owner, false);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null) return false;

        if (owner.Pets.Any(p => p.IsActive))
            throw new BusinessRuleException("Cannot delete owner with active pets. Deactivate pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner deleted: {OwnerId}", id);
        return true;
    }

    public async Task<List<PetSummaryDto>> GetPetsAsync(int ownerId, CancellationToken ct)
    {
        if (!await db.Owners.AnyAsync(o => o.Id == ownerId, ct))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive))
            .ToListAsync(ct);
    }

    public async Task<List<AppointmentDto>> GetAppointmentsAsync(int ownerId, CancellationToken ct)
    {
        if (!await db.Owners.AnyAsync(o => o.Id == ownerId, ct))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await db.Appointments
            .AsNoTracking()
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.VeterinarianId, a.AppointmentDate,
                a.DurationMinutes, a.Status.ToString(), a.Reason, a.Notes,
                a.CancellationReason, a.CreatedAt, a.UpdatedAt,
                new PetSummaryDto(a.Pet.Id, a.Pet.Name, a.Pet.Species, a.Pet.Breed, a.Pet.IsActive),
                null, null))
            .ToListAsync(ct);
    }

    private static OwnerDto MapToDto(Owner o, bool includePets) => new(
        o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
        o.Address, o.City, o.State, o.ZipCode,
        o.CreatedAt, o.UpdatedAt,
        includePets ? o.Pets.Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList() : null);
}
