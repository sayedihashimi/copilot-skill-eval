using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct = default);
    Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct = default);
    Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
}
