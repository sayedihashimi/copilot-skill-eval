using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PagedResponse<ClassScheduleResponse>> GetAllAsync(DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct);
    Task<List<ClassRosterResponse>> GetRosterAsync(int classId, CancellationToken ct);
    Task<List<ClassRosterResponse>> GetWaitlistAsync(int classId, CancellationToken ct);
    Task<List<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct);
}
