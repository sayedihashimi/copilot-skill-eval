using FitnessStudioApi.DTOs.Membership;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipService
{
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto> GetByIdAsync(int id);
    Task<MembershipDto> CancelAsync(int id);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipDto> UnfreezeAsync(int id);
    Task<MembershipDto> RenewAsync(int id);
}
