using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
    Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default);
    Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default);
    Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct = default);
}
