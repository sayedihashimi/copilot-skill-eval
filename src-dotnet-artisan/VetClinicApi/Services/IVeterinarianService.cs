using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct);
    Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct);
    Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct);
    Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct);
}
