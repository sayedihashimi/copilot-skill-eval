using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;

    public MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResult<MemberListDto>> GetAllAsync(int page, int pageSize, string? search, bool? isActive)
    {
        var query = _db.Members.AsQueryable();
        if (isActive.HasValue) query = query.Where(m => m.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m => m.FirstName.ToLower().Contains(s) ||
                                     m.LastName.ToLower().Contains(s) ||
                                     m.Email.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MemberListDto(m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.JoinDate, m.IsActive))
            .ToListAsync();

        return new PaginatedResult<MemberListDto>(items, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return null;

        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync();

        MembershipSummaryDto? summary = activeMembership == null ? null : new MembershipSummaryDto(
            activeMembership.Id, activeMembership.MembershipPlan.Name,
            activeMembership.StartDate, activeMembership.EndDate,
            activeMembership.Status.ToString());

        return new MemberDto(member.Id, member.FirstName, member.LastName, member.Email,
            member.Phone, member.DateOfBirth, member.EmergencyContactName,
            member.EmergencyContactPhone, member.JoinDate, member.IsActive,
            member.CreatedAt, member.UpdatedAt, summary);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        // Validate age >= 16
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth.AddYears(age) > today) age--;
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409, "Conflict");

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

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New member registered: {Name} ({Email})", $"{member.FirstName} {member.LastName}", member.Email);

        return (await GetByIdAsync(member.Id))!;
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new BusinessRuleException("Member not found.", 404, "Not Found");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409, "Conflict");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(member.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new BusinessRuleException("Member not found.", 404, "Not Found");

        var hasFutureBookings = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == id &&
                          b.Status == BookingStatus.Confirmed &&
                          b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResult<BookingDto>> GetBookingsAsync(int memberId, int page, int pageSize, string? status, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404, "Not Found");

        var query = _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var st))
            query = query.Where(b => b.Status == st);
        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<BookingDto>(
            items.Select(BookingServiceHelper.ToDto),
            total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404, "Not Found");

        var bookings = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                       b.Status == BookingStatus.Confirmed &&
                       b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync();

        return bookings.Select(BookingServiceHelper.ToDto);
    }

    public async Task<IEnumerable<MembershipDto>> GetMembershipsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404, "Not Found");

        var memberships = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync();

        return memberships.Select(MembershipServiceHelper.ToDto);
    }
}

public static class MembershipServiceHelper
{
    public static MembershipDto ToDto(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt);
}

public static class BookingServiceHelper
{
    public static BookingDto ToDto(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.CreatedAt, b.UpdatedAt);
}
