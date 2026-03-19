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

        return MapToResponse(vaccination);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync(ct);

        await db.Entry(vaccination).Reference(v => v.Pet).LoadAsync(ct);
        await db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync(ct);

        logger.LogInformation("Vaccination recorded with ID {VaccinationId}", vaccination.Id);
        return MapToResponse(vaccination);
    }

    private static VaccinationResponse MapToResponse(Vaccination v)
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
