using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PetService(VetClinicDbContext db) : IPetService
{
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PagedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species.ToLower() == species.ToLower());

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<PetResponse>
        {
            Items = items.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return pet is null ? null : MapToResponse(pet);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

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
        return MapToResponse(pet);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

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
        await db.Entry(pet).Reference(p => p.Owner).LoadAsync(ct);
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
        return true;
    }

    public async Task<List<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var records = await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return records.Select(MapMedicalRecord).ToList();
    }

    public async Task<List<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var vaccinations = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync(ct);

        return vaccinations.Select(MapVaccination).ToList();
    }

    public async Task<List<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        var vaccinations = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync(ct);

        return vaccinations.Select(MapVaccination).ToList();
    }

    public async Task<List<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == petId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var records = await db.MedicalRecords.AsNoTracking()
            .Where(m => m.PetId == petId)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var prescriptions = await db.Prescriptions.AsNoTracking()
            .Where(p => records.Contains(p.MedicalRecordId))
            .ToListAsync(ct);

        return prescriptions
            .Where(p => p.IsActive)
            .Select(MapPrescription)
            .ToList();
    }

    private static void ValidateSpecies(string species)
    {
        if (!ValidSpecies.Contains(species, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid species '{species}'. Must be one of: {string.Join(", ", ValidSpecies)}.");
    }

    private static PetResponse MapToResponse(Pet pet) => new()
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
        Owner = pet.Owner is null ? null : new OwnerSummaryResponse
        {
            Id = pet.Owner.Id,
            FirstName = pet.Owner.FirstName,
            LastName = pet.Owner.LastName,
            Email = pet.Owner.Email,
            Phone = pet.Owner.Phone
        },
        CreatedAt = pet.CreatedAt,
        UpdatedAt = pet.UpdatedAt
    };

    private static VaccinationResponse MapVaccination(Vaccination v) => new()
    {
        Id = v.Id,
        PetId = v.PetId,
        Pet = v.Pet is null ? null : new PetSummaryResponse { Id = v.Pet.Id, Name = v.Pet.Name, Species = v.Pet.Species, Breed = v.Pet.Breed, IsActive = v.Pet.IsActive },
        VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered,
        ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber,
        AdministeredByVetId = v.AdministeredByVetId,
        AdministeredByVet = v.AdministeredByVet is null ? null : new VeterinarianSummaryResponse { Id = v.AdministeredByVet.Id, FirstName = v.AdministeredByVet.FirstName, LastName = v.AdministeredByVet.LastName, Specialization = v.AdministeredByVet.Specialization },
        Notes = v.Notes,
        IsExpired = v.IsExpired,
        IsDueSoon = v.IsDueSoon,
        CreatedAt = v.CreatedAt
    };

    private static MedicalRecordResponse MapMedicalRecord(MedicalRecord m) => new()
    {
        Id = m.Id,
        AppointmentId = m.AppointmentId,
        PetId = m.PetId,
        Pet = m.Pet is null ? null : new PetSummaryResponse { Id = m.Pet.Id, Name = m.Pet.Name, Species = m.Pet.Species, Breed = m.Pet.Breed, IsActive = m.Pet.IsActive },
        VeterinarianId = m.VeterinarianId,
        Veterinarian = m.Veterinarian is null ? null : new VeterinarianSummaryResponse { Id = m.Veterinarian.Id, FirstName = m.Veterinarian.FirstName, LastName = m.Veterinarian.LastName, Specialization = m.Veterinarian.Specialization },
        Diagnosis = m.Diagnosis,
        Treatment = m.Treatment,
        Notes = m.Notes,
        FollowUpDate = m.FollowUpDate,
        CreatedAt = m.CreatedAt,
        Prescriptions = m.Prescriptions.Select(MapPrescription).ToList()
    };

    private static PrescriptionResponse MapPrescription(Prescription p) => new()
    {
        Id = p.Id,
        MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName,
        Dosage = p.Dosage,
        DurationDays = p.DurationDays,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        IsActive = p.IsActive,
        Instructions = p.Instructions,
        CreatedAt = p.CreatedAt
    };
}
