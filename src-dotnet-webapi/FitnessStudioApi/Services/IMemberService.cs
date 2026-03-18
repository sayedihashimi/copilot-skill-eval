using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PagedResponse<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
    Task<PagedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, BookingStatus? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct);
    Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<List<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct);
}
