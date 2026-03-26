using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorDto>> GetAllAsync(string? specialization, bool? isActive, CancellationToken ct = default);
    Task<InstructorDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto, CancellationToken ct = default);
    Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ClassScheduleListDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
