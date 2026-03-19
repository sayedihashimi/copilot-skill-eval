using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponseDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize);
    Task<AppointmentResponseDto> GetByIdAsync(int id);
    Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<List<AppointmentResponseDto>> GetTodayAsync();
}
