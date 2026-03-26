using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService : IPetService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext db, ILogger<PetService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination)
    {
        var query = _db.Pets.Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            var sp = species.ToLower();
            query = query.Where(p => p.Species.ToLower() == sp);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<PetResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PetResponseDto?> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        return pet == null ? null : MapToResponse(pet);
    }

    public async Task<PetResponseDto> CreateAsync(PetCreateDto dto)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new BusinessException("Owner not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) &&
            await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new BusinessException("A pet with this microchip number already exists.");

        var pet = new Pet
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth,
            Weight = dto.Weight,
            Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber,
            OwnerId = dto.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet created: {PetId} {Name}", pet.Id, pet.Name);

        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        return MapToResponse(pet);
    }

    public async Task<PetResponseDto?> UpdateAsync(int id, PetUpdateDto dto)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        if (pet == null) return null;

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new BusinessException("Owner not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) &&
            await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new BusinessException("A pet with this microchip number already exists.");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        return MapToResponse(pet);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null) return false;

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pet soft-deleted: {PetId}", id);
        return true;
    }

    public async Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId)
    {
        return await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MedicalRecordService.MapToResponse(m))
            .ToListAsync();
    }

    public async Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId)
    {
        var vaccinations = await _db.Vaccinations
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(30);

        var vaccinations = await _db.Vaccinations
            .Where(v => v.PetId == petId && v.ExpirationDate <= threshold)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync();

        return vaccinations.Select(VaccinationService.MapToResponse).ToList();
    }

    public async Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _db.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponseDto
            {
                Id = p.Id,
                MedicalRecordId = p.MedicalRecordId,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                DurationDays = p.DurationDays,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Instructions = p.Instructions,
                IsActive = true,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    private static PetResponseDto MapToResponse(Pet pet)
    {
        return new PetResponseDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            DateOfBirth = pet.DateOfBirth,
            Weight = pet.Weight,
            Color = pet.Color,
            MicrochipNumber = pet.MicrochipNumber,
            IsActive = pet.IsActive,
            OwnerId = pet.OwnerId,
            Owner = pet.Owner != null ? new OwnerSummaryDto
            {
                Id = pet.Owner.Id,
                FirstName = pet.Owner.FirstName,
                LastName = pet.Owner.LastName,
                Email = pet.Owner.Email,
                Phone = pet.Owner.Phone
            } : null,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt
        };
    }
}
