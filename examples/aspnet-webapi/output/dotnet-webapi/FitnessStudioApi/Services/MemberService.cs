using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    : IMemberService
{
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Members.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(s) ||
                m.LastName.ToLower().Contains(s) ||
                m.Email.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var members = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var memberIds = members.Select(m => m.Id).ToList();
        var activeMemberships = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Where(ms => memberIds.Contains(ms.MemberId) &&
                         (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .ToListAsync(ct);

        var items = members.Select(m =>
        {
            var activeMembership = activeMemberships.FirstOrDefault(ms => ms.MemberId == m.Id);
            return MapToResponse(m, activeMembership);
        }).ToList();

        return PaginatedResponse<MemberResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null)
            return null;

        var activeMembership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == id &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        return MapToResponse(member, activeMembership);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        var emailExists = await db.Members.AnyAsync(m => m.Email == request.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        if (request.DateOfBirth > minDob)
            throw new ArgumentException("Member must be at least 16 years old.");

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            JoinDate = DateOnly.FromDateTime(DateTime.Today)
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Registered new member {MemberName} with ID {MemberId}",
            $"{member.FirstName} {member.LastName}", member.Id);

        return MapToResponse(member, null);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var emailConflict = await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;

        await db.SaveChangesAsync(ct);

        var activeMembership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == id &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        logger.LogInformation("Updated member {MemberId}", member.Id);
        return MapToResponse(member, activeMembership);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = await db.Bookings
            .AnyAsync(b => b.MemberId == id &&
                           b.Status == BookingStatus.Confirmed &&
                           b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated member {MemberId}", member.Id);
    }

    public async Task<PaginatedResponse<BookingResponse>> GetBookingsAsync(
        int memberId, string? status, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            query = query.Where(b => b.Status == bookingStatus);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var bookings = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = bookings.Select(MapBookingToResponse).ToList();
        return PaginatedResponse<BookingResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                        b.Status == BookingStatus.Confirmed &&
                        b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipResponse(
                ms.Id, ms.MemberId,
                $"{ms.Member.FirstName} {ms.Member.LastName}",
                ms.MembershipPlanId, ms.MembershipPlan.Name,
                ms.StartDate, ms.EndDate,
                ms.Status, ms.PaymentStatus,
                ms.FreezeStartDate, ms.FreezeEndDate,
                ms.CreatedAt, ms.UpdatedAt))
            .ToListAsync(ct);
    }

    private static MemberResponse MapToResponse(Member m, Membership? activeMembership)
    {
        ActiveMembershipInfo? membershipInfo = activeMembership is null ? null :
            new ActiveMembershipInfo(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString());

        return new MemberResponse(
            m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
            m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
            m.JoinDate, m.IsActive, membershipInfo, m.CreatedAt, m.UpdatedAt);
    }

    private static BookingResponse MapBookingToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId,
            b.ClassSchedule.ClassType.Name,
            b.MemberId,
            $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status, b.WaitlistPosition,
            b.CheckInTime, b.CancellationDate, b.CancellationReason,
            b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
            b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
