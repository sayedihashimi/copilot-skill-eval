using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db) : IPetService
{
    public async Task<PagedResult<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Pets.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(p => p.Species.ToLower() == species.Trim().ToLower());
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<PetResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var pet = await db.Pets.AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pet is null)
        {
            return null;
        }

        var ownerDto = new OwnerResponse(
            pet.Owner.Id, pet.Owner.FirstName, pet.Owner.LastName, pet.Owner.Email, pet.Owner.Phone,
            pet.Owner.Address, pet.Owner.City, pet.Owner.State, pet.Owner.ZipCode,
            pet.Owner.CreatedAt, pet.Owner.UpdatedAt);

        return new PetDetailResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, ownerDto, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct = default)
    {
        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
        {
            throw new InvalidOperationException($"Owner with ID {request.OwnerId} not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct))
        {
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
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct = default)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return null;
        }

        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
        {
            throw new InvalidOperationException($"Owner with ID {request.OwnerId} not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct))
        {
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

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return false;
        }

        pet.IsActive = false;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct = default)
    {
        return await db.MedicalRecords.AsNoTracking()
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId, m.VeterinarianId,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
                v.Notes, v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
                v.Notes, v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= dueSoonDate,
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await db.Prescriptions.AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync(ct);

        return prescriptions
            .Where(p => p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt))
            .ToList();
    }
}
