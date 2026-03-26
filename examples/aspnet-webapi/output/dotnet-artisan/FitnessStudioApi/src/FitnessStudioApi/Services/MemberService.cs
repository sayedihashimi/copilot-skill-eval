using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db) : IMemberService
{
    public async Task<PaginatedResponse<MemberListResponse>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default)
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
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberListResponse(m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.IsActive, m.JoinDate))
            .ToListAsync(ct);

        return new PaginatedResponse<MemberListResponse>(items, totalCount, page, pageSize);
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members
            .AsNoTracking()
            .Include(m => m.Memberships)
                .ThenInclude(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null)
        {
            return null;
        }

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status is MembershipStatus.Active or MembershipStatus.Frozen);

        MembershipSummary? summary = null;
        if (activeMembership is not null)
        {
            summary = new MembershipSummary(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString());
        }

        return new MemberResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, summary, member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        var minDob = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-16);
        if (request.DateOfBirth > minDob)
        {
            throw new InvalidOperationException("Member must be at least 16 years old.");
        }

        if (await db.Members.AnyAsync(m => m.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        return new MemberResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, null, member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return false;
        }

        var hasFutureBookings = await db.Bookings
            .AnyAsync(b => b.MemberId == id &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
        {
            throw new InvalidOperationException("Cannot deactivate member with future bookings. Cancel bookings first.");
        }

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(
        int memberId, string? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
        {
            query = query.Where(b => b.Status == bookingStatus);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);

        return new PaginatedResponse<BookingResponse>(items, totalCount, page, pageSize);
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct = default)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");
        }

        return await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipResponse(
                ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
                ms.MembershipPlanId, ms.MembershipPlan.Name,
                ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
                ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt))
            .ToListAsync(ct);
    }

    internal static BookingResponse MapBookingToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        b.ClassSchedule.Room, $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
        b.CancellationDate, b.CancellationReason, b.CreatedAt, b.UpdatedAt);
}
