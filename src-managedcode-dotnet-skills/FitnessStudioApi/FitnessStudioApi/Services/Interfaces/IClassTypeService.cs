using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct = default);
    Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct = default);
}
