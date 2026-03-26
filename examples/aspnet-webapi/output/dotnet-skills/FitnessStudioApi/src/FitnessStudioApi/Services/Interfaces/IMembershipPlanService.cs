using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.MembershipPlan;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanDto>> GetAllAsync(bool? isActive = null);
    Task<MembershipPlanDto> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task DeleteAsync(int id);
}
