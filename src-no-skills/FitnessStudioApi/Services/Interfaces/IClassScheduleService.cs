using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? available, PaginationParams pagination);
    Task<ClassScheduleDto?> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto?> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto);
    Task<List<ClassRosterItemDto>> GetRosterAsync(int id);
    Task<List<ClassRosterItemDto>> GetWaitlistAsync(int id);
    Task<List<ClassScheduleDto>> GetAvailableAsync();
}
