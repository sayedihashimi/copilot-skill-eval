using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResult<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<BookingResponse>> GetBookingsAsync(int memberId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct = default);
}
