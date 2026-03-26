using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.Member;
using FitnessStudioApi.DTOs.Booking;
using FitnessStudioApi.DTOs.Membership;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMemberService
{
    Task<PaginatedResponse<MemberDto>> GetAllAsync(string? search, bool? isActive, int page = 1, int pageSize = 10);
    Task<MemberDetailDto> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
    Task DeleteAsync(int id);
    Task<PaginatedResponse<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10);
    Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<List<MembershipDto>> GetMembershipsAsync(int memberId);
}
