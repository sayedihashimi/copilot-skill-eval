using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResponse<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct);
    Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct);
    Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct);
    Task<List<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct);
    Task<PagedResponse<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct);
}
