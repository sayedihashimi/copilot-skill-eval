using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext db) : IOwnerService
{
    public async Task<PagedResult<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Owners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
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
            .Select(o => MapToResponse(o))
            .ToListAsync(ct);

        return new PagedResult<OwnerResponse>(items, totalCount, page, pageSize);
    }

    public async Task<OwnerDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners
            .AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null)
        {
            return null;
        }

        return new OwnerDetailResponse(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryResponse(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct = default)
    {
        if (await db.Owners.AnyAsync(o => o.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");
        }

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

        db.Owners.Add(owner);
        await db.SaveChangesAsync(ct);
        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct = default)
    {
        var owner = await db.Owners.FindAsync([id], ct);
        if (owner is null)
        {
            return null;
        }

        if (await db.Owners.AnyAsync(o => o.Email == request.Email && o.Id != id, ct))
        {
            throw new InvalidOperationException($"An owner with email '{request.Email}' already exists.");
        }

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Email = request.Email;
        owner.Phone = request.Phone;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;

        await db.SaveChangesAsync(ct);
        return MapToResponse(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null)
        {
            return false;
        }

        if (owner.Pets.Any(p => p.IsActive))
        {
            throw new InvalidOperationException("Cannot delete an owner that has active pets. Deactivate or transfer pets first.");
        }

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken ct = default)
    {
        return await db.Pets.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken ct = default)
    {
        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => MapAppointment(a))
            .ToListAsync(ct);
    }

    private static OwnerResponse MapToResponse(Owner o) =>
        new(o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
            o.Address, o.City, o.State, o.ZipCode, o.CreatedAt, o.UpdatedAt);

    private static AppointmentResponse MapAppointment(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status.ToString(),
            a.Reason, a.Notes, a.CancellationReason, a.CreatedAt, a.UpdatedAt);
}
