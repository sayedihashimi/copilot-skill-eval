using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PaginatedResponse<OwnerResponse>> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Owners.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o =>
                o.FirstName.Contains(search) ||
                o.LastName.Contains(search) ||
                o.Email.Contains(search));
        }

        query = query.OrderBy(o => o.LastName).ThenBy(o => o.FirstName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToResponse(o, null))
            .ToListAsync(ct);

        return PaginatedResponse<OwnerResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners
            .AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null) return null;

        var pets = owner.Pets.Select(p => new PetSummaryResponse(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList();
        return MapToResponse(owner, pets);
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct)
    {
        var existingEmail = await db.Owners.AnyAsync(o => o.Email == request.Email, ct);
        if (existingEmail)
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
            ZipCode = request.ZipCode
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner created with ID {OwnerId}", owner.Id);
        return MapToResponse(owner, null);
    }

    public async Task<OwnerResponse> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await db.Owners.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        var emailConflict = await db.Owners.AnyAsync(o => o.Email == request.Email && o.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Email = request.Email;
        owner.Phone = request.Phone;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner {OwnerId} updated", owner.Id);
        return MapToResponse(owner, null);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate all pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Owner {OwnerId} deleted", id);
    }

    public async Task<IReadOnlyList<PetResponse>> GetPetsByOwnerIdAsync(int ownerId, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.CreatedAt, p.UpdatedAt, null))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentResponse>> GetOwnerAppointmentsAsync(
        int ownerId, int page, int pageSize, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt, null))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    private static OwnerResponse MapToResponse(Owner o, IReadOnlyList<PetSummaryResponse>? pets) =>
        new(o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
            o.Address, o.City, o.State, o.ZipCode, o.CreatedAt, o.UpdatedAt, pets);
}
