using FitnessStudioApi.DTOs.ClassType;

namespace FitnessStudioApi.Services.Interfaces;

public interface IClassTypeService
{
    Task<List<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium);
    Task<ClassTypeDto> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto);
}
