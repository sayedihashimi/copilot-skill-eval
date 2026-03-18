using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService(VetClinicDbContext db) : IOwnerService
{
    public async Task<PagedResponse<OwnerSummaryResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Owners.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(s) ||
                o.LastName.ToLower().Contains(s) ||
                o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OwnerSummaryResponse
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Email = o.Email,
                Phone = o.Phone
            })
            .ToListAsync(ct);

        return new PagedResponse<OwnerSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners.AsNoTracking()
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

        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null) return null;

        var emailTaken = await db.Owners.AnyAsync(o => o.Email == request.Email && o.Id != id, ct);
        if (emailTaken)
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
        return MapToResponse(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (owner is null)
            throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate or transfer pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<PetSummaryResponse>> GetPetsAsync(int ownerId, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await db.Pets.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<AppointmentResponse>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == ownerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var petIds = await db.Pets.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => p.Id)
            .ToListAsync(ct);

        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => petIds.Contains(a.PetId))
            .OrderByDescending(a => a.AppointmentDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>
        {
            Items = items.Select(MapAppointment),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static OwnerResponse MapToResponse(Owner owner) => new()
    {
        Id = owner.Id,
        FirstName = owner.FirstName,
        LastName = owner.LastName,
        Email = owner.Email,
        Phone = owner.Phone,
        Address = owner.Address,
        City = owner.City,
        State = owner.State,
        ZipCode = owner.ZipCode,
        CreatedAt = owner.CreatedAt,
        UpdatedAt = owner.UpdatedAt,
        Pets = owner.Pets.Select(p => new PetSummaryResponse
        {
            Id = p.Id,
            Name = p.Name,
            Species = p.Species,
            Breed = p.Breed,
            IsActive = p.IsActive
        }).ToList()
    };

    private static AppointmentResponse MapAppointment(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        Pet = a.Pet is null ? null : new PetSummaryResponse { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
        VeterinarianId = a.VeterinarianId,
        Veterinarian = a.Veterinarian is null ? null : new VeterinarianSummaryResponse { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization },
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
