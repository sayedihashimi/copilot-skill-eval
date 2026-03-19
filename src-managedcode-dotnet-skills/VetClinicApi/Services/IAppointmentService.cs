using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<AppointmentResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct);
}
