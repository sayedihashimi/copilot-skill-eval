using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new BusinessRuleException("Member not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new BusinessRuleException("Class schedule not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("This class is not available for booking.");

        var now = DateTime.UtcNow;

        // Rule 1: Booking window (7 days advance, 30 min before)
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");

        if (schedule.StartTime < now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book classes less than 30 minutes before start time.");

        // Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m =>
                m.MemberId == request.MemberId &&
                m.Status == MembershipStatus.Active, ct)
            ?? throw new BusinessRuleException("Member does not have an active membership.");

        // Rule 4: Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not include access to premium classes.");

        // Rule 5: Weekly booking limits
        if (activeMembership.MembershipPlan.MaxClassBookingsPerWeek != -1)
        {
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var weeklyBookings = await db.Bookings.CountAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime >= startOfWeek &&
                b.ClassSchedule.StartTime < endOfWeek, ct);

            if (weeklyBookings >= activeMembership.MembershipPlan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException(
                    $"Weekly booking limit of {activeMembership.MembershipPlan.MaxClassBookingsPerWeek} reached.");
        }

        // Rule 7: No double booking (overlapping class times)
        var hasOverlap = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
            b.ClassSchedule.StartTime < schedule.EndTime &&
            b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class time.");

        // Check for existing booking in this class
        var existingBooking = await db.Bookings.AnyAsync(b =>
            b.ClassScheduleId == request.ClassScheduleId &&
            b.MemberId == request.MemberId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking)
            throw new BusinessRuleException("You are already booked for this class.");

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId
        };

        if (schedule.CurrentEnrollment < schedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            schedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            booking.WaitlistPosition = schedule.WaitlistCount + 1;
            schedule.WaitlistCount++;
        }

        schedule.UpdatedAt = DateTime.UtcNow;

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        await db.Entry(booking).Reference(b => b.Member).LoadAsync(ct);
        await db.Entry(booking).Reference(b => b.ClassSchedule).LoadAsync(ct);
        await db.Entry(booking.ClassSchedule).Reference(cs => cs.ClassType).LoadAsync(ct);

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ScheduleId} — Status: {Status}",
            booking.Id, booking.MemberId, booking.ClassScheduleId, booking.Status);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest? request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new BusinessRuleException("Booking not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new BusinessRuleException($"Cannot cancel a booking with status '{booking.Status}'.");

        var now = DateTime.UtcNow;

        // Rule 3: Cannot cancel started/completed classes
        if (booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking for a class that has started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = request?.Reason;
        booking.UpdatedAt = now;

        var schedule = booking.ClassSchedule;

        if (wasConfirmed)
        {
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Rule 2: Promote first waitlisted member
            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                schedule.CurrentEnrollment++;
                schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                // Re-number remaining waitlist
                var remaining = await db.Bookings
                    .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remaining.Count; i++)
                {
                    remaining[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted waitlisted booking {BookingId} to confirmed", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

            // Re-number remaining waitlist
            var remaining = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (var i = 0; i < remaining.Count; i++)
            {
                remaining[i].WaitlistPosition = i + 1;
            }
        }

        schedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled booking {BookingId}", booking.Id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new BusinessRuleException("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;

        // Rule 11: Check-in window is 15 min before to 15 min after class start
        var earliestCheckIn = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var latestCheckIn = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < earliestCheckIn)
            throw new BusinessRuleException("Check-in is not open yet. Check-in opens 15 minutes before class.");

        if (now > latestCheckIn)
            throw new BusinessRuleException("Check-in window has closed (15 minutes after class start).");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checked in booking {BookingId} at {Time}", booking.Id, now);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new BusinessRuleException("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        // Rule 12: No-show if not checked in 15 min after class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Marked booking {BookingId} as no-show", booking.Id);
        return MapToResponse(booking);
    }

    private static BookingResponse MapToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
        b.CancellationDate, b.CancellationReason,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
