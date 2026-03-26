using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleListDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct = default);
    Task<ClassScheduleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto, CancellationToken ct = default);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto, CancellationToken ct = default);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ClassScheduleListDto>> GetAvailableAsync(CancellationToken ct = default);
}
