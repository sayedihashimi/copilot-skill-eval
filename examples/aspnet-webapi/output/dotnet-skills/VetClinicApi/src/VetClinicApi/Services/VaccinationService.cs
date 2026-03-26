using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext context, ILogger<VaccinationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VaccinationDto?> GetByIdAsync(int id)
    {
        var vax = await _context.Vaccinations.FindAsync(id);
        if (vax == null) return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        return new VaccinationDto
        {
            Id = vax.Id,
            PetId = vax.PetId,
            VaccineName = vax.VaccineName,
            DateAdministered = vax.DateAdministered,
            ExpirationDate = vax.ExpirationDate,
            BatchNumber = vax.BatchNumber,
            AdministeredByVetId = vax.AdministeredByVetId,
            Notes = vax.Notes,
            IsExpired = vax.ExpirationDate < today,
            IsDueSoon = vax.ExpirationDate >= today && vax.ExpirationDate <= today.AddDays(30),
            CreatedAt = vax.CreatedAt
        };
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto)
    {
        var petExists = await _context.Pets.AnyAsync(p => p.Id == dto.PetId);
        if (!petExists) throw new InvalidOperationException("Pet not found.");

        var vetExists = await _context.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId);
        if (!vetExists) throw new InvalidOperationException("Veterinarian not found.");

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new InvalidOperationException("Expiration date must be after the date administered.");

        var today = DateOnly.FromDateTime(DateTime.Today);

        var vax = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes
        };

        _context.Vaccinations.Add(vax);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Vaccination recorded: {VaccinationId} {VaccineName} for Pet {PetId}", vax.Id, vax.VaccineName, vax.PetId);

        return new VaccinationDto
        {
            Id = vax.Id,
            PetId = vax.PetId,
            VaccineName = vax.VaccineName,
            DateAdministered = vax.DateAdministered,
            ExpirationDate = vax.ExpirationDate,
            BatchNumber = vax.BatchNumber,
            AdministeredByVetId = vax.AdministeredByVetId,
            Notes = vax.Notes,
            IsExpired = vax.ExpirationDate < today,
            IsDueSoon = vax.ExpirationDate >= today && vax.ExpirationDate <= today.AddDays(30),
            CreatedAt = vax.CreatedAt
        };
    }
}
