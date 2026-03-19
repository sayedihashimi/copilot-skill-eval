using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = _db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s)
                || o.LastName.ToLower().Contains(s)
                || o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new OwnerSummaryDto
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Email = o.Email,
                Phone = o.Phone
            }).ToListAsync();

        return new PagedResult<OwnerSummaryDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<OwnerResponseDto> GetByIdAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        return MapToResponse(owner);
    }

    public async Task<OwnerResponseDto> CreateAsync(CreateOwnerDto dto)
    {
        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email))
            throw new BusinessRuleException("An owner with this email already exists.", 409, "Conflict");

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

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created owner {OwnerId} ({Name})", owner.Id, $"{owner.FirstName} {owner.LastName}");

        return MapToResponse(owner);
    }

    public async Task<OwnerResponseDto> UpdateAsync(int id, UpdateOwnerDto dto)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email && o.Id != id))
            throw new BusinessRuleException("An owner with this email already exists.", 409, "Conflict");

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(owner);
    }

    public async Task DeleteAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found.");

        if (owner.Pets.Any(p => p.IsActive))
            throw new BusinessRuleException("Cannot delete owner with active pets. Deactivate or transfer pets first.");

        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted owner {OwnerId}", id);
    }

    public async Task<List<PetSummaryDto>> GetPetsAsync(int ownerId)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        return await _db.Pets.Where(p => p.OwnerId == ownerId)
            .Select(p => new PetSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                IsActive = p.IsActive
            }).ToListAsync();
    }

    public async Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found.");

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Reason = a.Reason,
                DurationMinutes = a.DurationMinutes,
                PetName = a.Pet.Name,
                VetName = a.Veterinarian.FirstName + " " + a.Veterinarian.LastName
            }).ToListAsync();

        return new PagedResult<AppointmentSummaryDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private static OwnerResponseDto MapToResponse(Owner owner) => new()
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
        Pets = owner.Pets?.Select(p => new PetSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Species = p.Species,
            Breed = p.Breed,
            IsActive = p.IsActive
        }).ToList() ?? new()
    };
}
