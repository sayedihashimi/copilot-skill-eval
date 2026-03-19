using System.Globalization;
using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IBookingService
{
    Task<BookingResponse> GetByIdAsync(int id);
    Task<BookingResponse> CreateAsync(CreateBookingRequest request);
    Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request);
    Task<BookingResponse> CheckInAsync(int id);
    Task<BookingResponse> MarkNoShowAsync(int id);
}

public class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> GetByIdAsync(int id)
    {
        var booking = await GetBookingWithRelations(id);
        return MemberService.MapBookingToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
    {
        var member = await db.Members.FindAsync(request.MemberId)
            ?? throw new NotFoundException($"Member with ID {request.MemberId} not found");

        if (!member.IsActive)
            throw new BusinessRuleException("Member is not active");

        var classSchedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId)
            ?? throw new NotFoundException($"Class schedule with ID {request.ClassScheduleId} not found");

        if (classSchedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("This class is not available for booking");

        // Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.MemberId == request.MemberId && m.Status == MembershipStatus.Active);

        if (activeMembership is null)
            throw new BusinessRuleException("Member does not have an active membership. Only members with active memberships can book classes.");

        var plan = activeMembership.MembershipPlan;

        // Rule 4: Membership tier access
        if (classSchedule.ClassType.IsPremium && !plan.AllowsPremiumClasses)
            throw new BusinessRuleException($"Your '{plan.Name}' plan does not include premium classes. Please upgrade to a plan that allows premium classes.");

        // Rule 1: Booking window (7 days advance, 30 min before)
        var now = DateTime.UtcNow;
        if (classSchedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance");

        if (classSchedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book classes less than 30 minutes before start time");

        // Rule 7: No double booking (overlapping times)
        var hasOverlap = await db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == request.MemberId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                          b.ClassSchedule.StartTime < classSchedule.EndTime &&
                          b.ClassSchedule.EndTime > classSchedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking that overlaps with this class time");

        // Rule 5: Weekly booking limits
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(classSchedule.StartTime);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b => b.MemberId == request.MemberId &&
                                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                                b.ClassSchedule.StartTime >= weekStart &&
                                b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookingCount >= plan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException($"You have reached your weekly booking limit of {plan.MaxClassBookingsPerWeek} classes for your '{plan.Name}' plan");
        }

        // Check for existing booking in same class
        var existingBooking = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.ClassScheduleId == request.ClassScheduleId &&
            b.Status != BookingStatus.Cancelled);

        if (existingBooking)
            throw new BusinessRuleException("You already have a booking for this class");

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId
        };

        if (classSchedule.CurrentEnrollment < classSchedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            classSchedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            booking.WaitlistPosition = classSchedule.WaitlistCount + 1;
            classSchedule.WaitlistCount++;
        }

        classSchedule.UpdatedAt = DateTime.UtcNow;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} - Status: {Status}",
            booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

        return MemberService.MapBookingToResponse(await GetBookingWithRelations(booking.Id));
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request)
    {
        var booking = await GetBookingWithRelations(id);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.NoShow)
            throw new BusinessRuleException($"Cannot cancel a booking with status '{booking.Status}'");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Rule 3: Cannot cancel started/completed classes
        if (classStart <= now && booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking for a class that has already started or completed");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        // Late cancellation warning (less than 2 hours before)
        string? cancellationNote = request.Reason;
        if (classStart - now < TimeSpan.FromHours(2))
            cancellationNote = $"Late cancellation. {request.Reason}".Trim();

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = cancellationNote;
        booking.UpdatedAt = now;

        var classSchedule = booking.ClassSchedule;

        if (wasConfirmed)
        {
            classSchedule.CurrentEnrollment = Math.Max(0, classSchedule.CurrentEnrollment - 1);

            // Promote first waitlisted person
            var firstWaitlisted = await db.Bookings
                .Include(b => b.Member)
                .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (firstWaitlisted is not null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = now;
                classSchedule.CurrentEnrollment++;
                classSchedule.WaitlistCount = Math.Max(0, classSchedule.WaitlistCount - 1);

                // Re-number remaining waitlist positions
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}", firstWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            classSchedule.WaitlistCount = Math.Max(0, classSchedule.WaitlistCount - 1);

            // Re-number remaining waitlist
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
            }
        }

        classSchedule.UpdatedAt = now;
        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled booking {BookingId}", id);

        return MemberService.MapBookingToResponse(await GetBookingWithRelations(id));
    }

    public async Task<BookingResponse> CheckInAsync(int id)
    {
        var booking = await GetBookingWithRelations(id);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Cannot check in a booking with status '{booking.Status}'. Only confirmed bookings can be checked in.");

        // Rule 11: Check-in window (15 min before to 15 min after class start)
        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var windowStart = classStart.AddMinutes(-15);
        var windowEnd = classStart.AddMinutes(15);

        if (now < windowStart)
            throw new BusinessRuleException("Check-in is not yet available. Check-in opens 15 minutes before class start time.");

        if (now > windowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is available from 15 minutes before to 15 minutes after class start time.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync();
        logger.LogInformation("Member {MemberId} checked in for class {ClassId}", booking.MemberId, booking.ClassScheduleId);
        return MemberService.MapBookingToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id)
    {
        var booking = await GetBookingWithRelations(id);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Cannot mark as no-show a booking with status '{booking.Status}'. Only confirmed bookings can be marked as no-show.");

        // Rule 12: Can only flag after 15 minutes past class start
        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (now < classStart.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync();
        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return MemberService.MapBookingToResponse(booking);
    }

    private async Task<Booking> GetBookingWithRelations(int id)
    {
        return await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Booking with ID {id} not found");
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // ISO week starts on Monday
        return date.Date.AddDays(-daysToSubtract);
    }
}
