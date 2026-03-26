using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vaccination = await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (vaccination is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isExpired = vaccination.ExpirationDate < today;
        var isDueSoon = !isExpired && vaccination.ExpirationDate <= today.AddDays(30);

        return new VaccinationResponse(
            vaccination.Id, vaccination.PetId, vaccination.Pet.Name,
            vaccination.VaccineName, vaccination.DateAdministered,
            vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId,
            vaccination.AdministeredByVet.FirstName + " " + vaccination.AdministeredByVet.LastName,
            vaccination.Notes, isExpired, isDueSoon, vaccination.CreatedAt);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");

        var vet = await db.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.Id == request.AdministeredByVetId, ct)
            ?? throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("ExpirationDate must be after DateAdministered.");

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes
        };

        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Vaccination recorded with ID {VaccinationId} for pet {PetId}",
            vaccination.Id, vaccination.PetId);

        var pet = await db.Pets.AsNoTracking().FirstAsync(p => p.Id == vaccination.PetId, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isExpired = vaccination.ExpirationDate < today;
        var isDueSoon = !isExpired && vaccination.ExpirationDate <= today.AddDays(30);

        return new VaccinationResponse(
            vaccination.Id, vaccination.PetId, pet.Name,
            vaccination.VaccineName, vaccination.DateAdministered,
            vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId,
            vet.FirstName + " " + vet.LastName,
            vaccination.Notes, isExpired, isDueSoon, vaccination.CreatedAt);
    }
}
