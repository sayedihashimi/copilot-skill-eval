using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    public async Task<PaginatedResponse<PetResponse>> GetAllAsync(
        string? name, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.CreatedAt, p.UpdatedAt, null))
            .ToListAsync(ct);

        return PaginatedResponse<PetResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets
            .AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pet is null) return null;

        var ownerSummary = new OwnerSummaryResponse(
            pet.Owner.Id, pet.Owner.FirstName, pet.Owner.LastName,
            pet.Owner.Email, pet.Owner.Phone);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.CreatedAt, pet.UpdatedAt, ownerSummary);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var microchipExists = await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct);
            if (microchipExists)
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
            OwnerId = request.OwnerId
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet created with ID {PetId}", pet.Id);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.CreatedAt, pet.UpdatedAt, null);
    }

    public async Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
        {
            var microchipConflict = await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct);
            if (microchipConflict)
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

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet {PetId} updated", pet.Id);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.CreatedAt, pet.UpdatedAt, null);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Pet {PetId} soft-deleted", id);
    }

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsByPetIdAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId,
                m.Pet.Name, m.VeterinarianId,
                m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                    p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow),
                    p.CreatedAt)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsByPetIdAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapVaccinationToResponse(v))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        return await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapVaccinationToResponse(v))
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
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                true, p.CreatedAt))
            .ToListAsync(ct);
    }

    private static VaccinationResponse MapVaccinationToResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isExpired = v.ExpirationDate < today;
        var isDueSoon = !isExpired && v.ExpirationDate <= today.AddDays(30);

        return new VaccinationResponse(
            v.Id, v.PetId, v.Pet.Name, v.VaccineName,
            v.DateAdministered, v.ExpirationDate, v.BatchNumber,
            v.AdministeredByVetId,
            v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
            v.Notes, isExpired, isDueSoon, v.CreatedAt);
    }
}
