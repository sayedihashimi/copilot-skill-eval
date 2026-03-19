using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize);
    Task<VeterinarianResponseDto> GetByIdAsync(int id);
    Task<VeterinarianResponseDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianResponseDto> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<List<AppointmentSummaryDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize);
}
