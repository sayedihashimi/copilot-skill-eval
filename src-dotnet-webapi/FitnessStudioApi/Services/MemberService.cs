using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MemberService(FitnessDbContext db) : IMemberService
{
    public async Task<PagedResponse<MemberListResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

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
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberListResponse
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Phone = m.Phone,
                IsActive = m.IsActive,
                JoinDate = m.JoinDate
            })
            .ToListAsync(ct);

        return new PagedResponse<MemberListResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null) return null;

        var activeMembership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

        var totalBookings = await db.Bookings.AsNoTracking()
            .CountAsync(b => b.MemberId == id, ct);

        var attendedClasses = await db.Bookings.AsNoTracking()
            .CountAsync(b => b.MemberId == id && b.Status == BookingStatus.Attended, ct);

        return new MemberResponse
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
            TotalBookings = totalBookings,
            AttendedClasses = attendedClasses,
            ActiveMembership = activeMembership is null ? null : MapMembershipToResponse(activeMembership),
            CreatedAt = member.CreatedAt,
            UpdatedAt = member.UpdatedAt
        };
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;

        if (age < 16)
            throw new ArgumentException("Member must be at least 16 years old.");

        var emailExists = await db.Members.AsNoTracking()
            .AnyAsync(m => m.Email == request.Email, ct);

        if (emailExists)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            JoinDate = today,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(member.Id, ct))!;
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;

        if (age < 16)
            throw new ArgumentException("Member must be at least 16 years old.");

        var emailExists = await db.Members.AsNoTracking()
            .AnyAsync(m => m.Email == request.Email && m.Id != id, ct);

        if (emailExists)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.DateOfBirth = request.DateOfBirth;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(member.Id, ct))!;
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = await db.Bookings.AsNoTracking()
            .AnyAsync(b => b.MemberId == id &&
                          b.Status == BookingStatus.Confirmed &&
                          b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, BookingStatus? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct)
    {
        var memberExists = await db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(b => b.ClassSchedule.StartTime >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(b => b.ClassSchedule.StartTime <= to);
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);

        return new PagedResponse<BookingResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await db.Bookings.AsNoTracking()
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

    public async Task<List<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AsNoTracking().AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        return await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MapMembershipToResponse(ms))
            .ToListAsync(ct);
    }

    private static MembershipResponse MapMembershipToResponse(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = $"{ms.Member.FirstName} {ms.Member.LastName}",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan.Name,
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status,
        PaymentStatus = ms.PaymentStatus,
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };

    private static BookingResponse MapBookingToResponse(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassTypeName = b.ClassSchedule.ClassType.Name,
        InstructorName = $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        ClassStartTime = b.ClassSchedule.StartTime,
        ClassEndTime = b.ClassSchedule.EndTime,
        Room = b.ClassSchedule.Room,
        MemberId = b.MemberId,
        MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
