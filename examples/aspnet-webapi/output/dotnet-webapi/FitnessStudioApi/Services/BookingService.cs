using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    : IBookingService
{
    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Inactive members cannot book classes.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new ArgumentException("Cannot book a class that is not scheduled.");

        // Business Rule 1: Booking window (7 days in advance, no less than 30 min before)
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
            throw new ArgumentException("Cannot book classes more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new ArgumentException("Cannot book classes less than 30 minutes before start time.");

        // Business Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == request.MemberId &&
                                       ms.Status == MembershipStatus.Active, ct)
            ?? throw new ArgumentException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Business Rule 4: Membership tier access for premium classes
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new ArgumentException(
                $"Your {activeMembership.MembershipPlan.Name} plan does not include access to premium classes. Upgrade to Premium or Elite to book this class.");

        // Business Rule 5: Weekly booking limits
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var isoWeek = ISOWeek.GetWeekOfYear(DateTime.Today);
            var isoYear = ISOWeek.GetYear(DateTime.Today);
            var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await db.Bookings
                .CountAsync(b => b.MemberId == request.MemberId &&
                                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                                b.ClassSchedule.StartTime >= weekStart &&
                                b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookingCount >= maxPerWeek)
                throw new ArgumentException(
                    $"Weekly booking limit reached ({maxPerWeek} per week for {activeMembership.MembershipPlan.Name} plan).");
        }

        // Business Rule 7: No double booking (overlapping times)
        var hasOverlap = await db.Bookings
            .AnyAsync(b => b.MemberId == request.MemberId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                          b.ClassSchedule.StartTime < schedule.EndTime &&
                          b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Member already has a booking that overlaps with this class time.");

        // Determine booking status based on capacity
        var isWaitlisted = schedule.CurrentEnrollment >= schedule.Capacity;

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            BookingDate = now,
            Status = isWaitlisted ? BookingStatus.Waitlisted : BookingStatus.Confirmed
        };

        if (isWaitlisted)
        {
            booking.WaitlistPosition = schedule.WaitlistCount + 1;
            schedule.WaitlistCount++;
        }
        else
        {
            schedule.CurrentEnrollment++;
        }

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        await db.Entry(booking).Reference(b => b.ClassSchedule).LoadAsync(ct);
        await db.Entry(booking.ClassSchedule).Reference(cs => cs.ClassType).LoadAsync(ct);
        await db.Entry(booking).Reference(b => b.Member).LoadAsync(ct);

        logger.LogInformation("Booking created: Member {MemberId} booked class {ClassId} - Status: {Status}",
            booking.MemberId, booking.ClassScheduleId, booking.Status);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new InvalidOperationException($"Cannot cancel a booking with status {booking.Status}.");

        var now = DateTime.UtcNow;

        // Business Rule 3: Cannot cancel after class started
        if (booking.ClassSchedule.StartTime <= now)
            throw new InvalidOperationException("Cannot cancel a class that has already started or completed.");

        var isLateCancellation = booking.ClassSchedule.StartTime <= now.AddHours(2);
        var cancellationReason = request.CancellationReason;
        if (isLateCancellation)
        {
            cancellationReason = $"[Late cancellation] {cancellationReason}".Trim();
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = cancellationReason;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first person from waitlist
            var nextInLine = await db.Bookings
                .Include(b => b.ClassSchedule)
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                           b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextInLine is not null)
            {
                nextInLine.Status = BookingStatus.Confirmed;
                nextInLine.WaitlistPosition = null;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Re-sequence remaining waitlist
                var remainingWaitlist = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                               b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remainingWaitlist.Count; i++)
                {
                    remainingWaitlist[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
                    nextInLine.Id, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Re-sequence remaining waitlist
            var remainingWaitlist = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                           b.Status == BookingStatus.Waitlisted &&
                           b.Id != booking.Id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (var i = 0; i < remainingWaitlist.Count; i++)
            {
                remainingWaitlist[i].WaitlistPosition = i + 1;
            }
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} cancelled{Late}",
            booking.Id, isLateCancellation ? " (late cancellation)" : "");

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Can only check in confirmed bookings.");

        var now = DateTime.UtcNow;

        // Business Rule 11: Check-in window is 15 min before to 15 min after class start
        var earliestCheckIn = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var latestCheckIn = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < earliestCheckIn)
            throw new ArgumentException("Check-in is not available yet. You can check in starting 15 minutes before class.");

        if (now > latestCheckIn)
            throw new ArgumentException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Member {MemberId} checked in for booking {BookingId}",
            booking.MemberId, booking.Id);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Can only mark confirmed bookings as no-show.");

        var now = DateTime.UtcNow;
        var noShowThreshold = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < noShowThreshold)
            throw new ArgumentException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} marked as no-show", booking.Id);
        return MapToResponse(booking);
    }

    private static BookingResponse MapToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId,
            b.ClassSchedule.ClassType.Name,
            b.MemberId,
            $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status, b.WaitlistPosition,
            b.CheckInTime, b.CancellationDate, b.CancellationReason,
            b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
            b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
