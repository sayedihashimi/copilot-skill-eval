using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext context) : IVaccinationService
{
    private readonly VetClinicDbContext _context = context;

    public async Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.Id == id)
            .Select(v => new VaccinationDto(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId,
                $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

        var petExists = await _context.Pets.AsNoTracking()
            .AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.Id == request.AdministeredByVetId, ct);
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
            Notes = request.Notes
        };

        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstAsync(v => v.Id == vaccination.Id, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationDto(
            created.Id, created.PetId, created.Pet.Name, created.VaccineName,
            created.DateAdministered, created.ExpirationDate, created.BatchNumber,
            created.AdministeredByVetId,
            $"{created.AdministeredByVet.FirstName} {created.AdministeredByVet.LastName}",
            created.Notes,
            created.ExpirationDate < today,
            created.ExpirationDate >= today && created.ExpirationDate <= today.AddDays(30),
            created.CreatedAt);
    }
}
