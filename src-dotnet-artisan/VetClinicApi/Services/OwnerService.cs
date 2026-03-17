using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext context) : IOwnerService
{
    private readonly VetClinicDbContext _context = context;

    public async Task<PaginatedResponse<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Owners.AsNoTracking().AsQueryable();

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
            .Select(o => MapToDto(o))
            .ToListAsync(ct);

        return new PaginatedResponse<OwnerDto>(items, page, pageSize, totalCount);
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var owner = await _context.Owners
            .AsNoTracking()
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null) return null;

        return new OwnerDetailDto(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerRequest request, CancellationToken ct)
    {
        var existingEmail = await _context.Owners.AsNoTracking()
            .AnyAsync(o => o.Email == request.Email, ct);
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

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync(ct);

        return MapToDto(owner);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await _context.Owners.FindAsync([id], ct);
        if (owner is null) return null;

        var emailTaken = await _context.Owners.AsNoTracking()
            .AnyAsync(o => o.Email == request.Email && o.Id != id, ct);
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

        await _context.SaveChangesAsync(ct);
        return MapToDto(owner);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var owner = await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (owner is null) return false;

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate all pets first.");

        _context.Owners.Remove(owner);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId, CancellationToken ct)
    {
        return await _context.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .Select(p => new PetDto(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Appointments
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
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentDto>(items, page, pageSize, totalCount);
    }

    private static OwnerDto MapToDto(Owner o) =>
        new(o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
            o.Address, o.City, o.State, o.ZipCode, o.CreatedAt, o.UpdatedAt);
}
