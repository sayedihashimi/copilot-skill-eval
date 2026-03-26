using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct);
    Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int classId, CancellationToken ct);
    Task<IReadOnlyList<ClassWaitlistEntry>> GetWaitlistAsync(int classId, CancellationToken ct);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct);
}
