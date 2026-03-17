using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleResponse>> GetAllAsync(DateTime? date, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct);
    Task<IReadOnlyList<RosterEntryResponse>> GetRosterAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<RosterEntryResponse>> GetWaitlistAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct);
}
