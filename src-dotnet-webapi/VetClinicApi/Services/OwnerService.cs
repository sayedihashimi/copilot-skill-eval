using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PaginatedResponse<OwnerSummaryResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
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
            .Select(o => new OwnerSummaryResponse(o.Id, o.FirstName, o.LastName, o.Email, o.Phone))
            .ToListAsync(ct);

        return PaginatedResponse<OwnerSummaryResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners
            .AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return owner is null ? null : MapToResponse(owner);
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct)
    {
        var exists = await db.Owners.AnyAsync(o => o.Email == request.Email, ct);
        if (exists)
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");

        var owner = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner created with ID {OwnerId}", owner.Id);
        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null) return null;

        var emailExists = await db.Owners.AnyAsync(o => o.Email == request.Email && o.Id != id, ct);
        if (emailExists)
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Email = request.Email;
        owner.Phone = request.Phone;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner updated with ID {OwnerId}", owner.Id);
        return MapToResponse(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null)
            throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate all pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner deleted with ID {OwnerId}", id);
        return true;
    }

    public async Task<IReadOnlyList<PetSummaryResponse>> GetPetsByOwnerIdAsync(int ownerId, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetSummaryResponse(p.Id, p.Name, p.Species, p.Breed, p.IsActive))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsByOwnerIdAsync(int ownerId, int page, int pageSize, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

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
            .Select(a => MapToAppointmentResponse(a))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    private static OwnerResponse MapToResponse(Owner owner) =>
        new(owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryResponse(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());

    private static AppointmentResponse MapToAppointmentResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason, a.CreatedAt, a.UpdatedAt);
}
