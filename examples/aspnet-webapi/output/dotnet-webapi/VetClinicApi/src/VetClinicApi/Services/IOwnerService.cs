using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct);
    Task<OwnerResponse> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PetResponse>> GetPetsByOwnerIdAsync(int ownerId, CancellationToken ct);
    Task<PaginatedResponse<AppointmentResponse>> GetOwnerAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct);
}
