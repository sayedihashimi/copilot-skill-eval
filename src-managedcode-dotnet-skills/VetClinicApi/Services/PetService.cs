using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext context, ILogger<PetService> logger) : IPetService
{
    public async Task<PagedResult<PetResponse>> GetAllAsync(int page, int pageSize, bool includeInactive, CancellationToken ct)
    {
        var query = context.Pets.AsNoTracking().Include(p => p.Owner).AsQueryable();
        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<PetResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Pets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        if (!await context.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
            throw new InvalidOperationException($"Owner with ID {request.OwnerId} not found.");

        if (request.MicrochipNumber is not null &&
            await context.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct))
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");

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

        context.Pets.Add(pet);
        await context.SaveChangesAsync(ct);
        await context.Entry(pet).Reference(p => p.Owner).LoadAsync(ct);

        logger.LogInformation("Created pet {PetId} ({PetName}) for owner {OwnerId}", pet.Id, pet.Name, pet.OwnerId);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.Owner.FirstName + " " + pet.Owner.LastName,
            pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null) return null;

        if (!await context.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
            throw new InvalidOperationException($"Owner with ID {request.OwnerId} not found.");

        if (request.MicrochipNumber is not null &&
            await context.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct))
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Weight = request.Weight;
        pet.Color = request.Color;
        pet.MicrochipNumber = request.MicrochipNumber;
        pet.OwnerId = request.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        await context.Entry(pet).Reference(p => p.Owner).LoadAsync(ct);

        logger.LogInformation("Updated pet {PetId}", pet.Id);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.Owner.FirstName + " " + pet.Owner.LastName,
            pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var pet = await context.Pets.FindAsync([id], ct);
        if (pet is null) return false;

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted pet {PetId}", id);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        return await context.MedicalRecords
            .AsNoTracking()
            .Include(mr => mr.Pet)
            .Include(mr => mr.Veterinarian)
            .Include(mr => mr.Prescriptions)
            .Where(mr => mr.PetId == petId)
            .OrderByDescending(mr => mr.CreatedAt)
            .Select(mr => new MedicalRecordResponse(
                mr.Id, mr.AppointmentId, mr.PetId, mr.Pet.Name,
                mr.VeterinarianId, mr.Veterinarian.FirstName + " " + mr.Veterinarian.LastName,
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt,
                mr.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                    p.DurationDays, p.StartDate, p.StartDate.AddDays(p.DurationDays),
                    p.StartDate.AddDays(p.DurationDays) >= DateOnly.FromDateTime(DateTime.UtcNow),
                    p.Instructions, p.CreatedAt)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDays = today.AddDays(30);
        return await context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate >= today && v.ExpirationDate <= thirtyDays)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes, false, true, v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.StartDate.AddDays(p.DurationDays) >= today)
            .OrderBy(p => p.StartDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.StartDate.AddDays(p.DurationDays),
                true, p.Instructions, p.CreatedAt))
            .ToListAsync(ct);
    }
}
