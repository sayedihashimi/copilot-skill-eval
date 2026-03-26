using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PaginatedResponse<PetResponse>> GetAllAsync(string? name, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsByPetIdAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsByPetIdAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}
