using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PagedResult<OwnerResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<OwnerResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken ct);
    Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken ct);
}
