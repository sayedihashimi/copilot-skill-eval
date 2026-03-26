using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(VaccinationDto? Vaccination, string? Error)> CreateAsync(CreateVaccinationDto dto, CancellationToken ct = default);
}

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var vaccination = await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (vaccination is null)
        {
            return null;
        }

        return MapToDto(vaccination);
    }

    public async Task<(VaccinationDto? Vaccination, string? Error)> CreateAsync(CreateVaccinationDto dto, CancellationToken ct = default)
    {
        if (dto.ExpirationDate <= dto.DateAdministered)
        {
            return (null, "Expiration date must be after date administered");
        }

        var petExists = await db.Pets.AnyAsync(p => p.Id == dto.PetId, ct);
        if (!petExists)
        {
            return (null, "Pet not found");
        }

        var vetExists = await db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId, ct);
        if (!vetExists)
        {
            return (null, "Veterinarian not found");
        }

        var vaccination = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes
        };

        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync(ct);

        await db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync(ct);

        logger.LogInformation("Vaccination recorded: {VaccinationId} for Pet {PetId}", vaccination.Id, vaccination.PetId);
        return (MapToDto(vaccination), null);
    }

    private static VaccinationDto MapToDto(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationDto(
            v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
            v.BatchNumber, v.AdministeredByVetId,
            $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes,
            v.ExpirationDate < today,
            v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            v.CreatedAt);
    }
}
