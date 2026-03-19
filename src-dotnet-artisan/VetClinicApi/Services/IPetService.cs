using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct = default);
    Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct = default);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct = default);
}
