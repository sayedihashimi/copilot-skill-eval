using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<PaginatedResult<MembershipPlanDto>> GetAllAsync(int page, int pageSize, bool? isActive);
    Task<MembershipPlanDto?> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task DeleteAsync(int id);
}

public interface IMemberService
{
    Task<PaginatedResult<MemberListDto>> GetAllAsync(int page, int pageSize, string? search, bool? isActive);
    Task<MemberDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
    Task DeleteAsync(int id);
    Task<PaginatedResult<BookingDto>> GetBookingsAsync(int memberId, int page, int pageSize, string? status, DateTime? fromDate, DateTime? toDate);
    Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<IEnumerable<MembershipDto>> GetMembershipsAsync(int memberId);
}

public interface IMembershipService
{
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto?> GetByIdAsync(int id);
    Task<MembershipDto> CancelAsync(int id);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipDto> UnfreezeAsync(int id);
    Task<MembershipDto> RenewAsync(int id);
}

public interface IInstructorService
{
    Task<PaginatedResult<InstructorDto>> GetAllAsync(int page, int pageSize, string? specialization, bool? isActive);
    Task<InstructorDto?> GetByIdAsync(int id);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto);
    Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<IEnumerable<ClassScheduleListDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to);
}

public interface IClassTypeService
{
    Task<PaginatedResult<ClassTypeDto>> GetAllAsync(int page, int pageSize, string? difficulty, bool? isPremium);
    Task<ClassTypeDto?> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto);
}

public interface IClassScheduleService
{
    Task<PaginatedResult<ClassScheduleListDto>> GetAllAsync(int page, int pageSize, DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available);
    Task<ClassScheduleDto?> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto);
    Task<IEnumerable<RosterEntryDto>> GetRosterAsync(int id);
    Task<IEnumerable<WaitlistEntryDto>> GetWaitlistAsync(int id);
    Task<IEnumerable<ClassScheduleListDto>> GetAvailableAsync();
}

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int id);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto);
    Task<BookingDto> CheckInAsync(int id);
    Task<BookingDto> NoShowAsync(int id);
}
