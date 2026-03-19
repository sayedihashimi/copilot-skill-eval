using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PagedResult<MemberResponse>> GetAllAsync(
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
            .Select(m => MapToResponse(m))
            .ToListAsync(ct);

        return new PagedResult<MemberResponse>(items, totalCount, page, pageSize);
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        return member is null ? null : MapToResponse(member);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        if (await db.Members.AnyAsync(m => m.Email == request.Email, ct))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists.");

        var age = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - request.DateOfBirth.DayNumber;
        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-16))
            throw new BusinessRuleException("Member must be at least 16 years old.");

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
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Registered new member {MemberId}: {Name}", member.Id, $"{member.FirstName} {member.LastName}");
        return MapToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return null;

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
            throw new BusinessRuleException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated member {MemberId}", member.Id);
        return MapToResponse(member);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return false;

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated member {MemberId}", member.Id);
        return true;
    }

    public async Task<IReadOnlyList<BookingResponse>> GetBookingsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new BusinessRuleException("Member not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new BusinessRuleException("Member not found.");

        var now = DateTime.UtcNow;
        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Where(b => b.MemberId == memberId &&
                        b.ClassSchedule.StartTime > now &&
                        b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new BusinessRuleException("Member not found.");

        return await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.StartDate)
            .Select(m => new MembershipResponse(
                m.Id, m.MemberId, $"{m.Member.FirstName} {m.Member.LastName}",
                m.MembershipPlanId, m.MembershipPlan.Name,
                m.StartDate, m.EndDate, m.Status, m.PaymentStatus,
                m.FreezeStartDate, m.FreezeEndDate, m.CreatedAt, m.UpdatedAt))
            .ToListAsync(ct);
    }

    private static MemberResponse MapToResponse(Member m) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
        m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
        m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt);

    private static BookingResponse MapBookingToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
        b.CancellationDate, b.CancellationReason,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
