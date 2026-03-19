using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
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

    public async Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize)
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
            query = query.Where(p => p.Species.ToLower() == species.ToLower());

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync();

        return new PagedResult<PetResponseDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PetResponseDto> GetByIdAsync(int id)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");
        return MapToResponse(pet);
    }

    public async Task<PetResponseDto> CreateAsync(CreatePetDto dto)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber))
            throw new BusinessRuleException("A pet with this microchip number already exists.", 409, "Conflict");

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

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created pet {PetId} ({Name})", pet.Id, pet.Name);

        await _db.Entry(pet).Reference(p => p.Owner).LoadAsync();
        return MapToResponse(pet);
    }

    public async Task<PetResponseDto> UpdateAsync(int id, UpdatePetDto dto)
    {
        var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        if (!await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId))
            throw new KeyNotFoundException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) && await _db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id))
            throw new BusinessRuleException("A pet with this microchip number already exists.", 409, "Conflict");

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

    public async Task DeleteAsync(int id)
    {
        var pet = await _db.Pets.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted pet {PetId}", id);
    }

    public async Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await _db.MedicalRecords
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponseDto
            {
                Id = m.Id,
                AppointmentId = m.AppointmentId,
                PetId = m.PetId,
                VeterinarianId = m.VeterinarianId,
                Diagnosis = m.Diagnosis,
                Treatment = m.Treatment,
                Notes = m.Notes,
                FollowUpDate = m.FollowUpDate,
                CreatedAt = m.CreatedAt,
                Prescriptions = m.Prescriptions.Select(p => new PrescriptionResponseDto
                {
                    Id = p.Id,
                    MedicalRecordId = p.MedicalRecordId,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    DurationDays = p.DurationDays,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Instructions = p.Instructions,
                    IsActive = p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
                    CreatedAt = p.CreatedAt
                }).ToList()
            }).ToListAsync();
    }

    public async Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapVaccination(v))
            .ToListAsync();
    }

    public async Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        return await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapVaccination(v))
            .ToListAsync();
    }

    public async Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

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
            }).ToListAsync();
    }

    private static PetResponseDto MapToResponse(Pet pet) => new()
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

    private static VaccinationResponseDto MapVaccination(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationResponseDto
        {
            Id = v.Id,
            PetId = v.PetId,
            VaccineName = v.VaccineName,
            DateAdministered = v.DateAdministered,
            ExpirationDate = v.ExpirationDate,
            BatchNumber = v.BatchNumber,
            AdministeredByVetId = v.AdministeredByVetId,
            AdministeredByVetName = v.AdministeredByVet != null ? $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}" : null,
            Notes = v.Notes,
            IsExpired = v.ExpirationDate < today,
            IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            CreatedAt = v.CreatedAt
        };
    }
}
