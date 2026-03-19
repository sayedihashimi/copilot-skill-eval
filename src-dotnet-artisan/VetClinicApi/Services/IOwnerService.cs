using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<OwnerDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct = default);
    Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken ct = default);
}
