using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<OwnerResponseDto> GetByIdAsync(int id);
    Task<OwnerResponseDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerResponseDto> UpdateAsync(int id, UpdateOwnerDto dto);
    Task DeleteAsync(int id);
    Task<List<PetSummaryDto>> GetPetsAsync(int ownerId);
    Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize);
}
