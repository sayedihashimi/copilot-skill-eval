using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberListDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    Task<MemberDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MemberDto> CreateAsync(CreateMemberDto dto, CancellationToken ct = default);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<PaginatedResponse<BookingDto>> GetMemberBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipDto>> GetMemberMembershipsAsync(int memberId, CancellationToken ct = default);
}
