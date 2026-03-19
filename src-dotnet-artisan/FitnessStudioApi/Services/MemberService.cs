using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IMemberService
{
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination);
    Task<MemberDetailResponse> GetByIdAsync(int id);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request);
    Task DeleteAsync(int id);
    Task<PaginatedResponse<BookingResponse>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, PaginationParams pagination);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId);
}

public class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination)
    {
        var query = db.Members.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(m => MapToResponse(m))
            .ToListAsync();

        return new PaginatedResponse<MemberResponse>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MemberDetailResponse> GetByIdAsync(int id)
    {
        var member = await db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException($"Member with ID {id} not found");

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status is MembershipStatus.Active or MembershipStatus.Frozen);

        MembershipSummary? membershipSummary = activeMembership is not null
            ? new MembershipSummary(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString())
            : null;

        var now = DateTime.UtcNow;
        return new MemberDetailResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, membershipSummary,
            member.Bookings.Count,
            member.Bookings.Count(b => b.Status == BookingStatus.Confirmed && b.ClassSchedule?.StartTime > now),
            member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old");

        if (await db.Members.AnyAsync(m => m.Email == request.Email))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists");

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone
        };

        db.Members.Add(member);
        await db.SaveChangesAsync();
        logger.LogInformation("Registered new member: {Email}", member.Email);
        return MapToResponse(member);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request)
    {
        var member = await db.Members.FindAsync(id)
            ?? throw new NotFoundException($"Member with ID {id} not found");

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.DateOfBirth = request.DateOfBirth;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated member: {MemberId}", id);
        return MapToResponse(member);
    }

    public async Task DeleteAsync(int id)
    {
        var member = await db.Members
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException($"Member with ID {id} not found");

        var hasFutureBookings = member.Bookings.Any(b =>
            b.Status == BookingStatus.Confirmed &&
            b.ClassSchedule?.StartTime > DateTime.UtcNow);

        // Load class schedules for future booking check
        var futureBookings = await db.Bookings
            .Include(b => b.ClassSchedule)
            .Where(b => b.MemberId == id && b.Status == BookingStatus.Confirmed)
            .AnyAsync(b => b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (futureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Deactivated member: {MemberId}", id);
    }

    public async Task<PaginatedResponse<BookingResponse>> GetBookingsAsync(
        int memberId, string? status, DateTime? from, DateTime? to, PaginationParams pagination)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found");

        var query = db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            query = query.Where(b => b.Status == bookingStatus);

        if (from.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<BookingResponse>
        {
            Items = items.Select(MapBookingToResponse).ToList(),
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found");

        var now = DateTime.UtcNow;
        var bookings = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync();

        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found");

        var memberships = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync();

        return memberships.Select(MapMembershipToResponse).ToList();
    }

    private static MemberResponse MapToResponse(Member m) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.DateOfBirth,
        m.EmergencyContactName, m.EmergencyContactPhone, m.JoinDate, m.IsActive,
        m.CreatedAt, m.UpdatedAt);

    internal static BookingResponse MapBookingToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.ClassSchedule.StartTime, b.MemberId,
        $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.CreatedAt, b.UpdatedAt);

    internal static MembershipResponse MapMembershipToResponse(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt);
}
