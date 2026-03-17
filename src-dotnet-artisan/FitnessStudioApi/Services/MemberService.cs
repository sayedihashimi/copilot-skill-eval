using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<MemberService> _logger = logger;

    public async Task<PagedResult<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Members.AsNoTracking().AsQueryable();

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

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MemberListResponse(m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.IsActive, m.JoinDate))
            .ToListAsync(ct);

        return new PagedResult<MemberListResponse>(items, totalCount, page, pageSize);
    }

    public async Task<MemberResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await _db.Members
            .AsNoTracking()
            .Include(m => m.Memberships)
                .ThenInclude(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        return MapToResponse(member);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        var age = CalculateAge(request.DateOfBirth);
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        var emailExists = await _db.Members.AnyAsync(m => m.Email == request.Email, ct);
        if (emailExists)
            throw new ConflictException($"A member with email '{request.Email}' already exists.");

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

        _logger.LogInformation("Registered member {Name} with ID {Id}", $"{member.FirstName} {member.LastName}", member.Id);
        return MapToResponse(member);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await _db.Members
            .Include(m => m.Memberships)
                .ThenInclude(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var emailConflict = await _db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct);
        if (emailConflict)
            throw new ConflictException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(member);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = await _db.Bookings.AnyAsync(
            b => b.MemberId == id &&
                 b.Status == BookingStatus.Confirmed &&
                 b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deactivated member {Id}", id);
    }

    public async Task<PagedResult<BookingResponse>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct)
    {
        _ = await _db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct)
            ? true : throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = _db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var statusEnum))
            query = query.Where(b => b.Status == statusEnum);
        if (from.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);

        return new PagedResult<BookingResponse>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        _ = await _db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct)
            ? true : throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
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
        _ = await _db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct)
            ? true : throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await _db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => new MembershipResponse(
                ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
                ms.MembershipPlanId, ms.MembershipPlan.Name,
                ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
                ms.FreezeStartDate, ms.FreezeEndDate,
                ms.CreatedAt, ms.UpdatedAt))
            .ToListAsync(ct);
    }

    private static int CalculateAge(DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }

    private static MemberResponse MapToResponse(Member m)
    {
        var activeMembership = m.Memberships?
            .Where(ms => ms.Status is MembershipStatus.Active or MembershipStatus.Frozen)
            .OrderByDescending(ms => ms.StartDate)
            .FirstOrDefault();

        ActiveMembershipInfo? activeMembershipInfo = activeMembership is not null
            ? new ActiveMembershipInfo(
                activeMembership.Id,
                activeMembership.MembershipPlan?.Name ?? "Unknown",
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString())
            : null;

        return new MemberResponse(
            m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
            m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
            m.JoinDate, m.IsActive, activeMembershipInfo,
            m.CreatedAt, m.UpdatedAt);
    }

    private static BookingResponse MapBookingToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
