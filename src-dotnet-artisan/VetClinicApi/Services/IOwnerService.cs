using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerDto>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<OwnerDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<OwnerDto> CreateAsync(CreateOwnerRequest request, CancellationToken ct);
    Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId, CancellationToken ct);
    Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize, CancellationToken ct);
}
