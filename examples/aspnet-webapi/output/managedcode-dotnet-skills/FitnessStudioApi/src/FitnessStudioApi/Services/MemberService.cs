using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberListDto>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Members.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberListDto(
                m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.JoinDate, m.IsActive))
            .ToListAsync(ct);

        return new PaginatedResponse<MemberListDto>(items, totalCount, page, pageSize);
    }

    public async Task<MemberDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null) return null;

        var activeMembership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

        MembershipSummaryDto? membershipSummary = activeMembership is not null
            ? new MembershipSummaryDto(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString())
            : null;

        return new MemberDto(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, membershipSummary);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        if (await db.Members.AnyAsync(m => m.Email == dto.Email, ct))
            throw new ConflictException($"A member with email '{dto.Email}' already exists.");

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

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Registered new member {MemberName} with Id {MemberId}",
            $"{member.FirstName} {member.LastName}", member.Id);

        return new MemberDto(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, null);
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new NotFoundException($"Member with Id {id} not found.");

        if (await db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id, ct))
            throw new ConflictException($"A member with email '{dto.Email}' already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated member {MemberId}", id);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new NotFoundException($"Member with Id {id} not found.");

        var hasFutureBookings = await db.Bookings
            .AnyAsync(b => b.MemberId == id &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated member {MemberId}", id);
    }

    public async Task<PaginatedResponse<BookingDto>> GetMemberBookingsAsync(
        int memberId, string? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new NotFoundException($"Member with Id {memberId} not found.");

        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToDto(b))
            .ToListAsync(ct);

        return new PaginatedResponse<BookingDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new NotFoundException($"Member with Id {memberId} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToDto(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipDto>> GetMemberMembershipsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new NotFoundException($"Member with Id {memberId} not found.");

        return await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipDto(
                ms.Id, ms.MemberId,
                $"{ms.Member.FirstName} {ms.Member.LastName}",
                ms.MembershipPlanId, ms.MembershipPlan.Name,
                ms.StartDate, ms.EndDate,
                ms.Status.ToString(), ms.PaymentStatus.ToString(),
                ms.FreezeStartDate, ms.FreezeEndDate))
            .ToListAsync(ct);
    }

    private static BookingDto MapBookingToDto(Booking b) => new(
        b.Id, b.ClassScheduleId,
        b.ClassSchedule.ClassType.Name,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        b.ClassSchedule.Room,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason);
}
