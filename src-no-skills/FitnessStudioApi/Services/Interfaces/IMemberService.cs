using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination);
    Task<MemberDetailDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto?> UpdateAsync(int id, UpdateMemberDto dto);
    Task<bool> DeactivateAsync(int id);
    Task<PagedResult<BookingDto>> GetMemberBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination);
    Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<List<MembershipDto>> GetMemberMembershipsAsync(int memberId);
}
