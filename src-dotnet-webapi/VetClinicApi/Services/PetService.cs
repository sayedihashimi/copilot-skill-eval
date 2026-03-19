using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    public async Task<PaginatedResponse<PetResponse>> GetAllAsync(string? name, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species.ToLower().Contains(species.ToLower()));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return PaginatedResponse<PetResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.AsNoTracking().Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id, ct);
        return pet is null ? null : MapToResponse(pet);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var chipExists = await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct);
            if (chipExists)
                throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            DateOfBirth = request.DateOfBirth,
            Weight = request.Weight,
            Color = request.Color,
            MicrochipNumber = request.MicrochipNumber,
            OwnerId = request.OwnerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        await db.Entry(pet).Reference(p => p.Owner).LoadAsync(ct);

        logger.LogInformation("Pet created with ID {PetId}", pet.Id);
        return MapToResponse(pet);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null) return null;

        var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var chipExists = await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct);
            if (chipExists)
                throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Weight = request.Weight;
        pet.Color = request.Color;
        pet.MicrochipNumber = request.MicrochipNumber;
        pet.OwnerId = request.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        if (pet.OwnerId != request.OwnerId)
            await db.Entry(pet).Reference(p => p.Owner).LoadAsync(ct);

        logger.LogInformation("Pet updated with ID {PetId}", pet.Id);
        return MapToResponse(pet);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null)
            throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet soft-deleted with ID {PetId}", id);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordSummaryResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.MedicalRecords
            .AsNoTracking()
            .Where(mr => mr.PetId == petId)
            .OrderByDescending(mr => mr.CreatedAt)
            .Select(mr => new MedicalRecordSummaryResponse(
                mr.Id, mr.AppointmentId, mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapToVaccinationResponse(v))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = today.AddDays(30);

        return await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= thirtyDaysFromNow)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapToVaccinationResponse(v))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await db.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .ToListAsync(ct);
    }

    private static PetResponse MapToResponse(Pet p) =>
        new(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color,
            p.MicrochipNumber, p.IsActive, p.OwnerId,
            new OwnerSummaryResponse(p.Owner.Id, p.Owner.FirstName, p.Owner.LastName, p.Owner.Email, p.Owner.Phone),
            p.CreatedAt, p.UpdatedAt);

    private static VaccinationResponse MapToVaccinationResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isExpired = v.ExpirationDate < today;
        var isDueSoon = !isExpired && v.ExpirationDate <= today.AddDays(30);
        return new VaccinationResponse(
            v.Id, v.PetId, v.Pet.Name, v.VaccineName,
            v.DateAdministered, v.ExpirationDate, v.BatchNumber,
            v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes, isExpired, isDueSoon, v.CreatedAt);
    }
}
