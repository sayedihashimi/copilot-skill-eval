using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext context, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.Id == id)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (!await context.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new InvalidOperationException($"Active pet with ID {request.PetId} not found.");

        if (!await context.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct))
            throw new InvalidOperationException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

        if (request.ExpirationDate <= request.DateAdministered)
            throw new InvalidOperationException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes,
        };

        context.Vaccinations.Add(vaccination);
        await context.SaveChangesAsync(ct);

        await context.Entry(vaccination).Reference(v => v.Pet).LoadAsync(ct);
        await context.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync(ct);

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationResponse(
            vaccination.Id, vaccination.PetId, vaccination.Pet.Name, vaccination.VaccineName,
            vaccination.DateAdministered, vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId,
            vaccination.AdministeredByVet.FirstName + " " + vaccination.AdministeredByVet.LastName,
            vaccination.Notes, vaccination.IsExpired, vaccination.IsDueSoon, vaccination.CreatedAt);
    }
}
