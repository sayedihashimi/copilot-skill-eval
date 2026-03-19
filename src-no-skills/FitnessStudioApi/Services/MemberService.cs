using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class MemberService : IMemberService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MemberService> _logger;

    public MemberService(FitnessDbContext context, ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination)
    {
        var query = _context.Members.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m => m.FirstName.ToLower().Contains(s)
                || m.LastName.ToLower().Contains(s)
                || m.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new PagedResult<MemberDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<MemberDetailDto?> GetByIdAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return null;

        var activeMembership = await _context.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync();

        var now = DateTime.UtcNow;
        var totalBookings = await _context.Bookings.CountAsync(b => b.MemberId == id);
        var upcomingBookings = await _context.Bookings
            .Where(b => b.MemberId == id && b.Status == BookingStatus.Confirmed)
            .Join(_context.ClassSchedules, b => b.ClassScheduleId, cs => cs.Id, (b, cs) => cs)
            .CountAsync(cs => cs.StartTime > now);

        return new MemberDetailDto
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
            UpdatedAt = member.UpdatedAt,
            ActiveMembership = activeMembership == null ? null : MapMembershipToDto(activeMembership),
            TotalBookings = totalBookings,
            UpcomingBookings = upcomingBookings
        };
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var minDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16));
        if (dto.DateOfBirth > minDate)
            throw new InvalidOperationException("Member must be at least 16 years old.");

        if (await _context.Members.AnyAsync(m => m.Email == dto.Email))
            throw new InvalidOperationException($"A member with email '{dto.Email}' already exists.");

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
        _logger.LogInformation("Registered new member: {Name} (ID: {MemberId})", $"{member.FirstName} {member.LastName}", member.Id);
        return MapToDto(member);
    }

    public async Task<MemberDto?> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return null;

        if (await _context.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new InvalidOperationException($"A member with email '{dto.Email}' already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(member);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return false;

        var hasFutureBookings = await _context.Bookings
            .Where(b => b.MemberId == id && b.Status == BookingStatus.Confirmed)
            .Join(_context.ClassSchedules, b => b.ClassScheduleId, cs => cs.Id, (b, cs) => cs)
            .AnyAsync(cs => cs.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deactivated member: {MemberId}", id);
        return true;
    }

    public async Task<PagedResult<BookingDto>> GetMemberBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(b => MapBookingToDto(b))
            .ToListAsync();

        return new PagedResult<BookingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var now = DateTime.UtcNow;
        return await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToDto(b))
            .ToListAsync();
    }

    public async Task<List<MembershipDto>> GetMemberMembershipsAsync(int memberId)
    {
        if (!await _context.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await _context.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MapMembershipToDto(ms))
            .ToListAsync();
    }

    private static MemberDto MapToDto(Member m) => new()
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
        UpdatedAt = m.UpdatedAt
    };

    private static MembershipDto MapMembershipToDto(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = ms.Member != null ? $"{ms.Member.FirstName} {ms.Member.LastName}" : "",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan?.Name ?? "",
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status,
        PaymentStatus = ms.PaymentStatus,
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };

    private static BookingDto MapBookingToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? "",
        MemberId = b.MemberId,
        MemberName = b.Member != null ? $"{b.Member.FirstName} {b.Member.LastName}" : "",
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        ClassStartTime = b.ClassSchedule?.StartTime ?? DateTime.MinValue,
        ClassEndTime = b.ClassSchedule?.EndTime ?? DateTime.MinValue,
        Room = b.ClassSchedule?.Room ?? "",
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
