using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<OwnerDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = _context.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s) ||
                                     o.LastName.ToLower().Contains(s) ||
                                     o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync();

        return new PagedResult<OwnerDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id)
    {
        var owner = await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (owner == null) return null;

        return new OwnerDetailDto
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
            Pets = owner.Pets.Select(p => new PetSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                IsActive = p.IsActive
            })
        };
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
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

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Owner created: {OwnerId} {Name}", owner.Id, $"{owner.FirstName} {owner.LastName}");
        return MapToDto(owner);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto)
    {
        var owner = await _context.Owners.FindAsync(id);
        if (owner == null) return null;

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Owner updated: {OwnerId}", owner.Id);
        return MapToDto(owner);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var owner = await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (owner == null) return false;

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate all pets first.");

        _context.Owners.Remove(owner);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Owner deleted: {OwnerId}", id);
        return true;
    }

    public async Task<IEnumerable<PetDto>> GetPetsAsync(int ownerId)
    {
        return await _context.Pets
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                DateOfBirth = p.DateOfBirth,
                Weight = p.Weight,
                Color = p.Color,
                MicrochipNumber = p.MicrochipNumber,
                IsActive = p.IsActive,
                OwnerId = p.OwnerId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination)
    {
        var query = _context.Appointments
            .Where(a => a.Pet.OwnerId == ownerId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PetId = a.PetId,
                VeterinarianId = a.VeterinarianId,
                AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes,
                Status = a.Status.ToString(),
                Reason = a.Reason,
                Notes = a.Notes,
                CancellationReason = a.CancellationReason,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    private static OwnerDto MapToDto(Owner owner) => new()
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
        UpdatedAt = owner.UpdatedAt
    };
}
