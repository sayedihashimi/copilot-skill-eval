using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct);
    Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int vetId, CancellationToken ct);
}
