using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PagedResult<PetResponseDto>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize);
    Task<PetResponseDto> GetByIdAsync(int id);
    Task<PetResponseDto> CreateAsync(CreatePetDto dto);
    Task<PetResponseDto> UpdateAsync(int id, UpdatePetDto dto);
    Task DeleteAsync(int id);
    Task<List<MedicalRecordResponseDto>> GetMedicalRecordsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetVaccinationsAsync(int petId);
    Task<List<VaccinationResponseDto>> GetUpcomingVaccinationsAsync(int petId);
    Task<List<PrescriptionResponseDto>> GetActivePrescriptionsAsync(int petId);
}
