using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingDto> CreateAsync(CreateBookingDto dto, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([dto.MemberId], ct)
            ?? throw new NotFoundException($"Member with Id {dto.MemberId} not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Inactive members cannot book classes.");

        var classSchedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId, ct)
            ?? throw new NotFoundException($"Class schedule with Id {dto.ClassScheduleId} not found.");

        if (classSchedule.Status != ClassStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{classSchedule.Status}'.");

        // Rule 1: Booking window — 7 days in advance, 30 min before start
        var now = DateTime.UtcNow;
        if (classSchedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");
        if (classSchedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book classes less than 30 minutes before start time.");

        // Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.MemberId == dto.MemberId &&
                m.Status == MembershipStatus.Active, ct)
            ?? throw new BusinessRuleException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Rule 4: Premium class access
        if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException(
                $"Your '{activeMembership.MembershipPlan.Name}' plan does not include premium classes. Upgrade to Premium or Elite to access this class.");

        // Rule 5: Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var bookingsThisWeek = await db.Bookings
                .CountAsync(b => b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd, ct);

            if (bookingsThisWeek >= maxPerWeek)
                throw new BusinessRuleException(
                    $"Weekly booking limit reached ({maxPerWeek} per week for '{activeMembership.MembershipPlan.Name}' plan).");
        }

        // Rule 7: No double booking — check time overlap
        var hasOverlap = await db.Bookings
            .AnyAsync(b => b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime < classSchedule.EndTime &&
                b.ClassSchedule.EndTime > classSchedule.StartTime, ct);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking that overlaps with this class time.");

        // Check if already booked for this class
        var existingBooking = await db.Bookings
            .AnyAsync(b => b.ClassScheduleId == dto.ClassScheduleId &&
                b.MemberId == dto.MemberId &&
                b.Status != BookingStatus.Cancelled, ct);

        if (existingBooking)
            throw new BusinessRuleException("You already have a booking for this class.");

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
            BookingDate = now
        };

        if (classSchedule.CurrentEnrollment < classSchedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            classSchedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            classSchedule.WaitlistCount++;
            booking.WaitlistPosition = classSchedule.WaitlistCount;
        }

        classSchedule.UpdatedAt = now;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking created: {BookingId} for member {MemberId} in class {ClassId}, status: {Status}",
            booking.Id, dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id, ct)
            ?? throw new InvalidOperationException("Failed to retrieve created booking.");
    }

    public async Task<BookingDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var b = await db.Bookings
            .AsNoTracking()
            .Include(x => x.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(x => x.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return b is null ? null : MapToDto(b);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Booking with Id {id} not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new BusinessRuleException($"Cannot cancel a booking with status '{booking.Status}'.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.StartTime <= now)
            throw new BusinessRuleException("Cannot cancel a booking for a class that has already started.");

        // Rule 3: Cancellation policy
        var isLateCancellation = booking.ClassSchedule.StartTime <= now.AddHours(2);
        var reason = dto.CancellationReason;
        if (isLateCancellation)
            reason = $"[Late cancellation] {reason ?? ""}".Trim();

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first from waitlist
            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                    b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;

                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Re-number remaining waitlist
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                        b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                    remainingWaitlisted[i].UpdatedAt = now;
                }

                logger.LogInformation("Promoted booking {PromotedBookingId} from waitlist for class {ClassId}",
                    nextWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else if (booking.Status == BookingStatus.Waitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Re-number remaining waitlist
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                    b.Status == BookingStatus.Waitlisted &&
                    b.Id != booking.Id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
                remainingWaitlisted[i].UpdatedAt = now;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled booking {BookingId} for class {ClassId}",
            id, booking.ClassScheduleId);

        return MapToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Booking with Id {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be checked in. Current status: '{booking.Status}'.");

        // Rule 11: Check-in window — 15 min before to 15 min after start
        var now = DateTime.UtcNow;
        var earliest = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var latest = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < earliest)
            throw new BusinessRuleException("Check-in is not yet available. Check-in opens 15 minutes before class start.");
        if (now > latest)
            throw new BusinessRuleException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Member {MemberId} checked in for class {ClassId}",
            booking.MemberId, booking.ClassScheduleId);

        return MapToDto(booking);
    }

    public async Task<BookingDto> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Booking with Id {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be marked as no-show. Current status: '{booking.Status}'.");

        // Rule 12: Can mark no-show after 15 min past class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Marked booking {BookingId} as no-show", id);

        return MapToDto(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var diff = (7 + (dayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id, b.ClassScheduleId,
        b.ClassSchedule.ClassType.Name,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        b.ClassSchedule.Room,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason);
}
