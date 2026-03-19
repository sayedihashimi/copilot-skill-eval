using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest? request, CancellationToken ct = default);
    Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<WaitlistEntry>> GetWaitlistAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct = default);
}
