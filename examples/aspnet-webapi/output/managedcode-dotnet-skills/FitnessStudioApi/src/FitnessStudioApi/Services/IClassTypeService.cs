using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium, CancellationToken ct = default);
    Task<ClassTypeDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto, CancellationToken ct = default);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto, CancellationToken ct = default);
}
