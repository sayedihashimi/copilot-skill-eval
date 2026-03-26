using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Vaccinations
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VaccinationDto(
                v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
                v.BatchNumber, v.AdministeredByVetId, v.Notes,
                v.ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow),
                v.ExpirationDate >= DateOnly.FromDateTime(DateTime.UtcNow) && v.ExpirationDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                v.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId, ct))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId, ct))
            throw new BusinessRuleException($"Veterinarian with ID {dto.AdministeredByVetId} not found.");

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new BusinessRuleException("Expiration date must be after the date administered.");

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

        logger.LogInformation("Vaccination recorded: {VaccinationId} {VaccineName} for pet {PetId}",
            vaccination.Id, vaccination.VaccineName, vaccination.PetId);
        return (await GetByIdAsync(vaccination.Id, ct))!;
    }
}
