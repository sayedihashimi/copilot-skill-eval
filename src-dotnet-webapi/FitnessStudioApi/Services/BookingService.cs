using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService : IBookingService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with id {request.MemberId} not found.");

        var classSchedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with id {request.ClassScheduleId} not found.");

        var now = DateTime.UtcNow;

        // Rule 6: Active Membership Required
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active, ct)
            ?? throw new BusinessRuleException("Member does not have an active membership. Only members with active memberships can book classes.");

        // Check class is bookable
        if (classSchedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{classSchedule.Status}'.");

        // Rule 1: Booking Window - 7 days in advance, no less than 30 min before
        if (classSchedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");

        if (classSchedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");

        // Rule 4: Membership Tier Access
        if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow booking premium classes. Please upgrade to Premium or Elite.");

        // Rule 5: Weekly Booking Limits
        var plan = activeMembership.MembershipPlan;
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(classSchedule.StartTime);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b =>
                    b.MemberId == request.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookings >= plan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException($"Weekly booking limit reached ({plan.MaxClassBookingsPerWeek} per week for {plan.Name} plan).");
        }

        // Rule 7: No Double Booking
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < classSchedule.EndTime &&
                b.ClassSchedule.EndTime > classSchedule.StartTime, ct);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class at this time.", 409);

        // Rule 2: Capacity Management
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
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
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Booking {BookingId} created for member {MemberId} in class {ClassId} with status {Status}",
            booking.Id, member.Id, classSchedule.Id, booking.Status);

        return ToResponse(booking, classSchedule, member);
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await _db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : ToResponse(booking, booking.ClassSchedule, booking.Member);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new BusinessRuleException("Cannot cancel a booking that has been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;

        // Rule 3: Cannot cancel started/completed class
        if (booking.ClassSchedule.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking for a completed class.");

        if (booking.ClassSchedule.StartTime <= now && booking.ClassSchedule.Status == ClassScheduleStatus.InProgress)
            throw new BusinessRuleException("Cannot cancel a booking for a class that has already started.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        // Rule 3: Late cancellation marking
        var reason = request.Reason ?? "";
        if (booking.ClassSchedule.StartTime <= now.AddHours(2) && booking.ClassSchedule.StartTime > now)
            reason = string.IsNullOrEmpty(reason) ? "Late cancellation (less than 2 hours before class)" : $"{reason} [Late cancellation]";

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first waitlisted booking
            var firstWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (firstWaitlisted is not null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await _db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                _logger.LogInformation("Booking {WaitlistBookingId} promoted from waitlist for class {ClassId}",
                    firstWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Reorder waitlist
            var remainingWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted && b.Id != booking.Id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Booking {BookingId} cancelled for class {ClassId}", id, booking.ClassScheduleId);
        return ToResponse(booking, booking.ClassSchedule, booking.Member);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;

        // Rule 11: Check-in window: 15 min before to 15 min after start
        if (now < booking.ClassSchedule.StartTime.AddMinutes(-15))
            throw new BusinessRuleException("Check-in is only available starting 15 minutes before class start time.");

        if (now > booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Check-in window has closed (15 minutes after class start time).");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Member {MemberId} checked in for class {ClassId}", booking.MemberId, booking.ClassScheduleId);
        return ToResponse(booking, booking.ClassSchedule, booking.Member);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        // Rule 12: Only flag as no-show after 15 min past start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Booking {BookingId} marked as no-show for class {ClassId}", id, booking.ClassScheduleId);
        return ToResponse(booking, booking.ClassSchedule, booking.Member);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var day = date.Date;
        var diff = (7 + (day.DayOfWeek - DayOfWeek.Monday)) % 7;
        return day.AddDays(-diff);
    }

    private static BookingResponse ToResponse(Booking b, ClassSchedule cs, Member m) => new(
        b.Id, b.ClassScheduleId, cs.ClassType.Name,
        b.MemberId, $"{m.FirstName} {m.LastName}",
        b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        cs.StartTime, cs.EndTime, cs.Room,
        b.CreatedAt, b.UpdatedAt);
}
