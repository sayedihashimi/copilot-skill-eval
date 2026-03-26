using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<MembershipPlanDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto, CancellationToken ct = default);
    Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
