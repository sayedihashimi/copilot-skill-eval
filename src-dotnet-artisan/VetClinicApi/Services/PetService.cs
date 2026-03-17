using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext context) : IPetService
{
    private readonly VetClinicDbContext _context = context;
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PaginatedResponse<PetDto>> GetAllAsync(
        string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Pets.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetDto(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<PetDto>(items, page, pageSize, totalCount);
    }

    public async Task<PetDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Pets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDetailDto(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, $"{p.Owner.FirstName} {p.Owner.LastName}",
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PetDto> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

        var ownerExists = await _context.Owners.AsNoTracking()
            .AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (request.MicrochipNumber is not null)
        {
            var chipExists = await _context.Pets.AsNoTracking()
                .AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct);
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
            OwnerId = request.OwnerId
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync(ct);

        return new PetDto(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetDto?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

        var pet = await _context.Pets.FindAsync([id], ct);
        if (pet is null) return null;

        var ownerExists = await _context.Owners.AsNoTracking()
            .AnyAsync(o => o.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (request.MicrochipNumber is not null)
        {
            var chipExists = await _context.Pets.AsNoTracking()
                .AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct);
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

        await _context.SaveChangesAsync(ct);

        return new PetDto(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var pet = await _context.Pets.FindAsync([id], ct);
        if (pet is null) return false;

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        return await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Veterinarian)
            .Include(m => m.Pet)
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapToMedicalRecordDto(m))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        return await _context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => MapToVaccinationDto(v))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        return await _context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => MapToVaccinationDto(v))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .ToListAsync(ct);
    }

    private static void ValidateSpecies(string species)
    {
        if (!ValidSpecies.Contains(species, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid species '{species}'. Valid values: {string.Join(", ", ValidSpecies)}");
    }

    private static MedicalRecordDto MapToMedicalRecordDto(MedicalRecord m) =>
        new(m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
            m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
            m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
            m.Prescriptions.Select(p => new PrescriptionDto(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow), p.CreatedAt)).ToList());

    private static VaccinationDto MapToVaccinationDto(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationDto(
            v.Id, v.PetId, v.Pet.Name, v.VaccineName,
            v.DateAdministered, v.ExpirationDate, v.BatchNumber,
            v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes,
            v.ExpirationDate < today,
            v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            v.CreatedAt);
    }
}
