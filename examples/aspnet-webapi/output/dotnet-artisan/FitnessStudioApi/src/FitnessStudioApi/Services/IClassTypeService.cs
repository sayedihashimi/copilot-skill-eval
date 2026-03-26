using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassTypeService
{
    Task<List<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, CancellationToken ct = default);
    Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct = default);
    Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct = default);
}
