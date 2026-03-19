using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = context.Owners.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToResponse(o))
            .ToListAsync(ct);

        return new PagedResult<OwnerResponse>(items, totalCount, page, pageSize);
    }

    public async Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Owners
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => MapToResponse(o))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct)
    {
        if (await context.Owners.AnyAsync(o => o.Email == request.Email, ct))
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
        };

        context.Owners.Add(owner);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created owner {OwnerId} ({Email})", owner.Id, owner.Email);
        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await context.Owners.FindAsync([id], ct);
        if (owner is null) return null;

        if (await context.Owners.AnyAsync(o => o.Email == request.Email && o.Id != id, ct))
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

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Updated owner {OwnerId}", owner.Id);
        return MapToResponse(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await context.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null) return false;

        if (owner.Pets.Count != 0)
            throw new InvalidOperationException("Cannot delete owner with associated pets. Remove or transfer pets first.");

        context.Owners.Remove(owner);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Deleted owner {OwnerId}", id);
        return true;
    }

    public async Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken ct)
    {
        return await context.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId && p.IsActive)
            .Include(p => p.Owner)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken ct)
    {
        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    private static OwnerResponse MapToResponse(Owner o) => new(
        o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
        o.Address, o.City, o.State, o.ZipCode,
        o.CreatedAt, o.UpdatedAt, o.Pets.Count);
}
