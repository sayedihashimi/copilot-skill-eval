using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipService
{
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto, CancellationToken ct = default);
    Task<MembershipDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MembershipDto> CancelAsync(int id, CancellationToken ct = default);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto, CancellationToken ct = default);
    Task<MembershipDto> UnfreezeAsync(int id, CancellationToken ct = default);
    Task<MembershipDto> RenewAsync(int id, CancellationToken ct = default);
}
