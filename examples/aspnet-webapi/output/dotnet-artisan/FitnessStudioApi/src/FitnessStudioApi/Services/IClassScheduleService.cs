using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleListResponse>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct = default);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct = default);
    Task<List<RosterEntryResponse>> GetRosterAsync(int classId, CancellationToken ct = default);
    Task<List<WaitlistEntryResponse>> GetWaitlistAsync(int classId, CancellationToken ct = default);
    Task<List<ClassScheduleListResponse>> GetAvailableAsync(CancellationToken ct = default);
}
