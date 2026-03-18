using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService(VetClinicDbContext db) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vaccination = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        return vaccination is null ? null : MapToResponse(vaccination);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

        var petExists = await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

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

        return MapToResponse(vaccination);
    }

    private static VaccinationResponse MapToResponse(Vaccination v) => new()
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
}
