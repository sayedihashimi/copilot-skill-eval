using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.ClassSchedule;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, int page = 1, int pageSize = 10);
    Task<ClassScheduleDto> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto);
    Task<List<RosterEntryDto>> GetRosterAsync(int id);
    Task<List<RosterEntryDto>> GetWaitlistAsync(int id);
    Task<List<ClassScheduleDto>> GetAvailableClassesAsync();
}
