using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorResponse>> GetAllAsync(CancellationToken ct = default);
    Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default);
    Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, CancellationToken ct = default);
}
