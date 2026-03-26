using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    public async Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species.ToLower() == species.ToLower());

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetDto(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.CreatedAt, p.UpdatedAt, null))
            .ToListAsync(ct);

        return new PagedResult<PetDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PetDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Pets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDto(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.CreatedAt, p.UpdatedAt,
                new OwnerSummaryDto(p.Owner.Id, p.Owner.FirstName, p.Owner.LastName, p.Owner.Email, p.Owner.Phone)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct)
    {
        if (!await db.Owners.AnyAsync(o => o.Id == dto.OwnerId, ct))
            throw new BusinessRuleException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber, ct))
            throw new BusinessRuleException("A pet with this microchip number already exists.", StatusCodes.Status409Conflict, "Duplicate Microchip");

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

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet created: {PetId} {Name} for owner {OwnerId}", pet.Id, pet.Name, pet.OwnerId);
        return (await GetByIdAsync(pet.Id, ct))!;
    }

    public async Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null) return null;

        if (!await db.Owners.AnyAsync(o => o.Id == dto.OwnerId, ct))
            throw new BusinessRuleException($"Owner with ID {dto.OwnerId} not found.");

        if (!string.IsNullOrEmpty(dto.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == dto.MicrochipNumber && p.Id != id, ct))
            throw new BusinessRuleException("A pet with this microchip number already exists.", StatusCodes.Status409Conflict, "Duplicate Microchip");

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.DateOfBirth = dto.DateOfBirth;
        pet.Weight = dto.Weight;
        pet.Color = dto.Color;
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.OwnerId = dto.OwnerId;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null) return false;

        pet.IsActive = false;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet soft-deleted: {PetId}", id);
        return true;
    }

    public async Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.MedicalRecords
            .AsNoTracking()
            .Where(m => m.PetId == petId)
            .Include(m => m.Prescriptions)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto(
                m.Id, m.AppointmentId, m.PetId, m.VeterinarianId,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionDto(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<List<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.Vaccinations
            .AsNoTracking()
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapVaccinationDto(v))
            .ToListAsync(ct);
    }

    public async Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soonDate = today.AddDays(30);

        return await db.Vaccinations
            .AsNoTracking()
            .Where(v => v.PetId == petId && v.ExpirationDate <= soonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapVaccinationDto(v))
            .ToListAsync(ct);
    }

    public async Task<List<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await db.Prescriptions
            .AsNoTracking()
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                true, p.CreatedAt))
            .ToListAsync(ct);
    }

    private static VaccinationDto MapVaccinationDto(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationDto(
            v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
            v.BatchNumber, v.AdministeredByVetId, v.Notes,
            v.ExpirationDate < today,
            v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            v.CreatedAt);
    }
}
