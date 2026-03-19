using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetResponse>> GetAllAsync(int page, int pageSize, bool includeInactive, CancellationToken ct);
    Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}
