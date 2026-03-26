using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService : IPetService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<PetService> _logger;

    public PetService(VetClinicDbContext context, ILogger<PetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination)
    {
        var query = _context.Pets.AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(p => p.Species.ToLower() == species.ToLower());
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<PetDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PetDetailDto?> GetByIdAsync(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null) return null;

        return new PetDetailDto
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
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,
            Owner = new OwnerDto
            {
                Id = pet.Owner.Id,
                FirstName = pet.Owner.FirstName,
                LastName = pet.Owner.LastName,
                Email = pet.Owner.Email,
                Phone = pet.Owner.Phone,
                Address = pet.Owner.Address,
                City = pet.Owner.City,
                State = pet.Owner.State,
                ZipCode = pet.Owner.ZipCode,
                CreatedAt = pet.Owner.CreatedAt,
                UpdatedAt = pet.Owner.UpdatedAt
            }
        };
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto)
    {
        var ownerExists = await _context.Owners.AnyAsync(o => o.Id == dto.OwnerId);
        if (!ownerExists) throw new InvalidOperationException("Owner not found.");

        var pet = new Pet
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            DateOfBirth = dto.DateOfBirth,
            Weight = dto.Weight,
            Color = dto.Color,
            MicrochipNumber = dto.MicrochipNumber,
            OwnerId = dto.OwnerId
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pet created: {PetId} {Name}", pet.Id, pet.Name);
        return MapToDto(pet);
    }

    public async Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return null;

        var ownerExists = await _context.Owners.AnyAsync(o => o.Id == dto.OwnerId);
        if (!ownerExists) throw new InvalidOperationException("Owner not found.");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Pet updated: {PetId}", pet.Id);
        return MapToDto(pet);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return false;

        pet.IsActive = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pet soft-deleted: {PetId}", id);
        return true;
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsAsync(int petId)
    {
        return await _context.MedicalRecords
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto
            {
                Id = m.Id,
                AppointmentId = m.AppointmentId,
                PetId = m.PetId,
                VeterinarianId = m.VeterinarianId,
                Diagnosis = m.Diagnosis,
                Treatment = m.Treatment,
                Notes = m.Notes,
                FollowUpDate = m.FollowUpDate,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<VaccinationDto>> GetVaccinationsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Vaccinations
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationDto
            {
                Id = v.Id,
                PetId = v.PetId,
                VaccineName = v.VaccineName,
                DateAdministered = v.DateAdministered,
                ExpirationDate = v.ExpirationDate,
                BatchNumber = v.BatchNumber,
                AdministeredByVetId = v.AdministeredByVetId,
                Notes = v.Notes,
                IsExpired = v.ExpirationDate < today,
                IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var soon = today.AddDays(30);
        return await _context.Vaccinations
            .Where(v => v.PetId == petId && (v.ExpirationDate < today || (v.ExpirationDate >= today && v.ExpirationDate <= soon)))
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationDto
            {
                Id = v.Id,
                PetId = v.PetId,
                VaccineName = v.VaccineName,
                DateAdministered = v.DateAdministered,
                ExpirationDate = v.ExpirationDate,
                BatchNumber = v.BatchNumber,
                AdministeredByVetId = v.AdministeredByVetId,
                Notes = v.Notes,
                IsExpired = v.ExpirationDate < today,
                IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= soon,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PrescriptionDto>> GetActivePrescriptionsAsync(int petId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Prescriptions
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionDto
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

    private static PetDto MapToDto(Pet pet) => new()
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
        CreatedAt = pet.CreatedAt,
        UpdatedAt = pet.UpdatedAt
    };
}
