using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
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

    public async Task<VaccinationResponseDto> GetByIdAsync(int id)
    {
        var v = await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vaccination with ID {id} not found.");
        return MapToResponse(v);
    }

    public async Task<VaccinationResponseDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.AdministeredByVetId} not found.");

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

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Recorded vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);

        await _db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync();
        return MapToResponse(vaccination);
    }

    private static VaccinationResponseDto MapToResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationResponseDto
        {
            Id = v.Id,
            PetId = v.PetId,
            VaccineName = v.VaccineName,
            DateAdministered = v.DateAdministered,
            ExpirationDate = v.ExpirationDate,
            BatchNumber = v.BatchNumber,
            AdministeredByVetId = v.AdministeredByVetId,
            AdministeredByVetName = v.AdministeredByVet != null ? $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}" : null,
            Notes = v.Notes,
            IsExpired = v.ExpirationDate < today,
            IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            CreatedAt = v.CreatedAt
        };
    }
}
