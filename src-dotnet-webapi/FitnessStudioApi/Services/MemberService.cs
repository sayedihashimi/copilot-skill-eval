using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;

    public MemberService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Members.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(s) ||
                m.LastName.ToLower().Contains(s) ||
                m.Email.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => ToResponse(m))
            .ToListAsync(ct);

        return new PaginatedResponse<MemberResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await _db.Members.AsNoTracking()
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null) return null;

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen);

        MembershipResponse? activeMembershipResponse = activeMembership is null ? null : new MembershipResponse(
            activeMembership.Id, activeMembership.MemberId,
            $"{member.FirstName} {member.LastName}",
            activeMembership.MembershipPlanId, activeMembership.MembershipPlan.Name,
            activeMembership.StartDate, activeMembership.EndDate,
            activeMembership.Status.ToString(), activeMembership.PaymentStatus.ToString(),
            activeMembership.FreezeStartDate, activeMembership.FreezeEndDate,
            activeMembership.CreatedAt, activeMembership.UpdatedAt);

        return new MemberDetailResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, activeMembershipResponse,
            member.Bookings.Count,
            member.Bookings.Count(b => b.Status == BookingStatus.Attended),
            member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        // Age validation
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        if (await _db.Members.AnyAsync(m => m.Email == request.Email, ct))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists.", 409);

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

        _db.Members.Add(member);
        await _db.SaveChangesAsync(ct);
        return ToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([id], ct);
        if (member is null) return null;

        if (await _db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists.", 409);

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.DateOfBirth = request.DateOfBirth;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToResponse(member);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await _db.Members
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null) return false;

        var hasFutureBookings = member.Bookings.Any(b =>
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with active bookings. Cancel bookings first.", 409);

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(
        int memberId, string? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with id {memberId} not found.");

        var query = _db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => ToBookingResponse(b))
            .ToListAsync(ct);

        return new PaginatedResponse<BookingResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with id {memberId} not found.");

        var now = DateTime.UtcNow;
        return await _db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => ToBookingResponse(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with id {memberId} not found.");

        return await _db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipResponse(
                ms.Id, ms.MemberId,
                ms.Member.FirstName + " " + ms.Member.LastName,
                ms.MembershipPlanId, ms.MembershipPlan.Name,
                ms.StartDate, ms.EndDate,
                ms.Status.ToString(), ms.PaymentStatus.ToString(),
                ms.FreezeStartDate, ms.FreezeEndDate,
                ms.CreatedAt, ms.UpdatedAt))
            .ToListAsync(ct);
    }

    private static MemberResponse ToResponse(Member m) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
        m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
        m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt);

    private static BookingResponse ToBookingResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, b.Member.FirstName + " " + b.Member.LastName,
        b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime, b.ClassSchedule.Room,
        b.CreatedAt, b.UpdatedAt);
}
