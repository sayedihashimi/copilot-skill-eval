using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<VaccinationResponseDto?> GetByIdAsync(int id)
    {
        var vaccination = await _db.Vaccinations.FindAsync(id);
        return vaccination == null ? null : MapToResponse(vaccination);
    }

    public async Task<VaccinationResponseDto> CreateAsync(VaccinationCreateDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new BusinessException("Pet not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new BusinessException("Veterinarian not found.");
        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new BusinessException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Vaccination recorded: {VaccinationId} for Pet {PetId}", vaccination.Id, vaccination.PetId);

        return MapToResponse(vaccination);
    }

    internal static VaccinationResponseDto MapToResponse(Vaccination v)
    {
        return new VaccinationResponseDto
        {
            Id = v.Id,
            PetId = v.PetId,
            VaccineName = v.VaccineName,
            DateAdministered = v.DateAdministered,
            ExpirationDate = v.ExpirationDate,
            BatchNumber = v.BatchNumber,
            AdministeredByVetId = v.AdministeredByVetId,
            Notes = v.Notes,
            IsExpired = v.IsExpired,
            IsDueSoon = v.IsDueSoon,
            CreatedAt = v.CreatedAt
        };
    }
}
