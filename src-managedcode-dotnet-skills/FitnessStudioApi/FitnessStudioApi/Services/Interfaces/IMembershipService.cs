using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipService
{
    Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct = default);
    Task<MembershipResponse> CancelAsync(int id, CancelMembershipRequest? request, CancellationToken ct = default);
    Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct = default);
    Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct = default);
    Task<MembershipResponse> RenewAsync(int id, CancellationToken ct = default);
}
