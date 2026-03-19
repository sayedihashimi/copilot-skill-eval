using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct = default);
    Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct = default);
    Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct = default);
    Task<PagedResult<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct = default);
}
