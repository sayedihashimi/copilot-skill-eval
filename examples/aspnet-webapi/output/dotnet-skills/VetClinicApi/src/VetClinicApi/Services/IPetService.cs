using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetDto>> GetAllAsync(string? search, string? species, bool includeInactive, PaginationParams pagination);
    Task<PetDetailDto?> GetByIdAsync(int id);
    Task<PetDto> CreateAsync(CreatePetDto dto);
    Task<PetDto?> UpdateAsync(int id, UpdatePetDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsAsync(int petId);
    Task<IEnumerable<VaccinationDto>> GetVaccinationsAsync(int petId);
    Task<IEnumerable<VaccinationDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<IEnumerable<PrescriptionDto>> GetActivePrescriptionsAsync(int petId);
}
