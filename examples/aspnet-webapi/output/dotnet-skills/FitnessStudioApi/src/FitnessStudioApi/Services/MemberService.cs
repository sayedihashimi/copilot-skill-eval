using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.Booking;
using FitnessStudioApi.DTOs.Member;
using FitnessStudioApi.DTOs.Membership;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService : IMemberService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MemberService> _logger;

    public MemberService(FitnessDbContext context, ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<MemberDto>> GetAllAsync(string? search, bool? isActive, int page = 1, int pageSize = 10)
    {
        var query = _context.Members.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(m =>
                EF.Functions.Like(m.FirstName, pattern) ||
                EF.Functions.Like(m.LastName, pattern) ||
                EF.Functions.Like(m.Email, pattern));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new PaginatedResponse<MemberDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MemberDetailDto> GetByIdAsync(int id)
    {
        // Use projection to avoid loading all bookings/memberships into memory
        var dto = await _context.Members.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MemberDetailDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Phone = m.Phone,
                DateOfBirth = m.DateOfBirth,
                EmergencyContactName = m.EmergencyContactName,
                EmergencyContactPhone = m.EmergencyContactPhone,
                JoinDate = m.JoinDate,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                TotalBookings = m.Bookings.Count,
                AttendedClasses = m.Bookings.Count(b => b.Status == BookingStatus.Attended),
                ActiveMembership = m.Memberships
                    .Where(ms => ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen)
                    .Select(ms => new MembershipSummaryDto
                    {
                        Id = ms.Id,
                        PlanName = ms.MembershipPlan.Name,
                        Status = ms.Status.ToString(),
                        StartDate = ms.StartDate,
                        EndDate = ms.EndDate
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        return dto;
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409, "Duplicate Resource");

        var member = new Member
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered new member: {MemberName} (ID: {MemberId})", $"{member.FirstName} {member.LastName}", member.Id);
        return MapToDto(member);
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _context.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        if (await _context.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409, "Duplicate Resource");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated member: {MemberName} (ID: {MemberId})", $"{member.FirstName} {member.LastName}", member.Id);
        return MapToDto(member);
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _context.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = await _context.Bookings
            .AnyAsync(b => b.MemberId == id
                && b.Status == BookingStatus.Confirmed
                && b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated member: {MemberName} (ID: {MemberId})", $"{member.FirstName} {member.LastName}", member.Id);
    }

    public async Task<PaginatedResponse<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = _context.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            query = query.Where(b => b.Status == bookingStatus);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToDto(b))
            .ToListAsync();

        return new PaginatedResponse<BookingDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await _context.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId
                && b.Status == BookingStatus.Confirmed
                && b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToDto(b))
            .ToListAsync();
    }

    public async Task<List<MembershipDto>> GetMembershipsAsync(int memberId)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await _context.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipDto
            {
                Id = ms.Id,
                MemberId = ms.MemberId,
                MemberName = $"{ms.Member.FirstName} {ms.Member.LastName}",
                MembershipPlanId = ms.MembershipPlanId,
                PlanName = ms.MembershipPlan.Name,
                StartDate = ms.StartDate,
                EndDate = ms.EndDate,
                Status = ms.Status.ToString(),
                PaymentStatus = ms.PaymentStatus.ToString(),
                FreezeStartDate = ms.FreezeStartDate,
                FreezeEndDate = ms.FreezeEndDate,
                CreatedAt = ms.CreatedAt,
                UpdatedAt = ms.UpdatedAt
            })
            .ToListAsync();
    }

    private static MemberDto MapToDto(Member member) => new()
    {
        Id = member.Id,
        FirstName = member.FirstName,
        LastName = member.LastName,
        Email = member.Email,
        Phone = member.Phone,
        DateOfBirth = member.DateOfBirth,
        EmergencyContactName = member.EmergencyContactName,
        EmergencyContactPhone = member.EmergencyContactPhone,
        JoinDate = member.JoinDate,
        IsActive = member.IsActive,
        CreatedAt = member.CreatedAt,
        UpdatedAt = member.UpdatedAt
    };

    private static BookingDto MapBookingToDto(Booking booking) => new()
    {
        Id = booking.Id,
        ClassScheduleId = booking.ClassScheduleId,
        ClassName = booking.ClassSchedule.ClassType.Name,
        MemberId = booking.MemberId,
        MemberName = $"{booking.Member.FirstName} {booking.Member.LastName}",
        BookingDate = booking.BookingDate,
        Status = booking.Status.ToString(),
        WaitlistPosition = booking.WaitlistPosition,
        CheckInTime = booking.CheckInTime,
        CancellationDate = booking.CancellationDate,
        CancellationReason = booking.CancellationReason,
        ClassStartTime = booking.ClassSchedule.StartTime,
        ClassEndTime = booking.ClassSchedule.EndTime,
        Room = booking.ClassSchedule.Room,
        InstructorName = $"{booking.ClassSchedule.Instructor.FirstName} {booking.ClassSchedule.Instructor.LastName}",
        CreatedAt = booking.CreatedAt,
        UpdatedAt = booking.UpdatedAt
    };
}
