using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PagedResult<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
    Task<PagedResult<BookingResponse>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct);
}
