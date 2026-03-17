using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<VaccinationDto> CreateAsync(CreateVaccinationRequest request, CancellationToken ct);
}
