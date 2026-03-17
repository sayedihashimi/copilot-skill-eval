using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct);
}
