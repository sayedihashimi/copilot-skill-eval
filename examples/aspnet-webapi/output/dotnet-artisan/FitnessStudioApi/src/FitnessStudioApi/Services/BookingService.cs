using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
        {
            throw new InvalidOperationException("Member account is inactive.");
        }

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot book a class that is {schedule.Status}.");
        }

        var now = DateTime.UtcNow;

        // Booking window: up to 7 days in advance, no less than 30 minutes before
        if (schedule.StartTime > now.AddDays(7))
        {
            throw new InvalidOperationException("Cannot book a class more than 7 days in advance.");
        }

        if (schedule.StartTime <= now.AddMinutes(30))
        {
            throw new InvalidOperationException("Cannot book a class less than 30 minutes before start time.");
        }

        // Active membership required
        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active, ct)
            ?? throw new InvalidOperationException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Premium class access check
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
        {
            throw new InvalidOperationException(
                $"Your {activeMembership.MembershipPlan.Name} plan does not include access to premium classes. Upgrade to Premium or Elite to book this class.");
        }

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await db.Bookings
                .CountAsync(b => b.MemberId == request.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookings >= maxPerWeek)
            {
                throw new InvalidOperationException(
                    $"Weekly booking limit reached ({maxPerWeek} classes per week on your {activeMembership.MembershipPlan.Name} plan).");
            }
        }

        // No double booking (overlapping times)
        var hasOverlap = await db.Bookings
            .AnyAsync(b => b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime < schedule.EndTime &&
                b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
        {
            throw new InvalidOperationException("You already have a booking for an overlapping class at this time.");
        }

        // Check for existing booking for the same class
        var existingBooking = await db.Bookings
            .FirstOrDefaultAsync(b => b.MemberId == request.MemberId &&
                b.ClassScheduleId == request.ClassScheduleId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking is not null)
        {
            throw new InvalidOperationException("You already have a booking for this class.");
        }

        // Create booking - either confirmed or waitlisted
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
        };

        if (schedule.CurrentEnrollment < schedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            schedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            schedule.WaitlistCount++;
            booking.WaitlistPosition = schedule.WaitlistCount;
        }

        schedule.UpdatedAt = DateTime.UtcNow;

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} created for member {MemberId} in class {ClassId} - Status: {Status}",
            booking.Id, member.Id, schedule.Id, booking.Status);

        return await GetByIdAsync(booking.Id, ct)
            ?? throw new InvalidOperationException("Failed to retrieve created booking.");
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MemberService.MapBookingToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.NoShow or BookingStatus.Attended)
        {
            throw new InvalidOperationException($"Cannot cancel a booking with status {booking.Status}.");
        }

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (classStart <= now)
        {
            throw new InvalidOperationException("Cannot cancel a class that has already started or completed.");
        }

        var isLateCancellation = (classStart - now).TotalHours < 2;
        var reason = request.Reason ?? "Cancelled by member";
        if (isLateCancellation)
        {
            reason += " (late cancellation)";
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first waitlisted person
            var nextOnWaitlist = await db.Bookings
                .Include(b => b.Member)
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                    b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextOnWaitlist is not null)
            {
                nextOnWaitlist.Status = BookingStatus.Confirmed;
                nextOnWaitlist.WaitlistPosition = null;
                nextOnWaitlist.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Re-number remaining waitlist positions
                var remainingWaitlist = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                        b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remainingWaitlist.Count; i++)
                {
                    remainingWaitlist[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Waitlisted member {MemberId} promoted to confirmed for class {ClassId}",
                    nextOnWaitlist.MemberId, booking.ClassScheduleId);
            }
        }
        else if (booking.Status == BookingStatus.Cancelled && booking.WaitlistPosition.HasValue)
        {
            booking.ClassSchedule.WaitlistCount--;
            booking.WaitlistPosition = null;

            // Re-number remaining waitlist positions
            var remainingWaitlist = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                    b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (var i = 0; i < remainingWaitlist.Count; i++)
            {
                remainingWaitlist[i].WaitlistPosition = i + 1;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} cancelled{Late}", id, isLateCancellation ? " (late)" : "");

        return MemberService.MapBookingToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot check in a booking with status {booking.Status}. Only confirmed bookings can be checked in.");
        }

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var windowStart = classStart.AddMinutes(-15);
        var windowEnd = classStart.AddMinutes(15);

        if (now < windowStart)
        {
            throw new InvalidOperationException("Check-in is not available yet. Check-in opens 15 minutes before class start.");
        }

        if (now > windowEnd)
        {
            throw new InvalidOperationException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");
        }

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Member {MemberId} checked in for booking {BookingId}", booking.MemberId, id);

        return MemberService.MapBookingToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot mark as no-show a booking with status {booking.Status}. Only confirmed bookings can be marked as no-show.");
        }

        var now = DateTime.UtcNow;
        var noShowThreshold = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < noShowThreshold)
        {
            throw new InvalidOperationException("Cannot mark as no-show before 15 minutes after class start time.");
        }

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} marked as no-show", id);

        return MemberService.MapBookingToResponse(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var diff = (7 + (dayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
