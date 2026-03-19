using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanDto>> GetAllAsync();
    Task<MembershipPlanDto?> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto?> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task<bool> DeactivateAsync(int id);
}
