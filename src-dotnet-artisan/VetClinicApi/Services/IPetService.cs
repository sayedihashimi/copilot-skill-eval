using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PaginatedResponse<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetDto> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetDto?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MedicalRecordDto>> GetMedicalRecordsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationDto>> GetVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct);
    Task<IReadOnlyList<PrescriptionDto>> GetActivePrescriptionsAsync(int petId, CancellationToken ct);
}
