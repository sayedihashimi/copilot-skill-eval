using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponse>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct = default);
    Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct = default);
    Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct = default);
    Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct = default);
}
