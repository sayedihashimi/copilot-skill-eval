using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<BookingService> _logger = logger;

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        var now = DateTime.UtcNow;

        // Rule: Class must be Scheduled
        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Cannot book a class that is not in Scheduled status.");

        // Rule 1: Booking window - max 7 days in advance
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book more than 7 days in advance.");

        // Rule 1: No less than 30 min before class start
        if (schedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book less than 30 minutes before class start.");

        // Rule 6: Active membership required
        var activeMembership = await _db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.MemberId == request.MemberId &&
                                      m.Status == MembershipStatus.Active, ct)
            ?? throw new BusinessRuleException("Member does not have an active membership. Frozen memberships cannot book.");

        // Rule 4: Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow premium class bookings.");

        // Rule 5: Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var (weekStart, weekEnd) = GetIsoWeekRange(schedule.StartTime);

            var weeklyBookingCount = await _db.Bookings.CountAsync(
                b => b.MemberId == request.MemberId &&
                     (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                     b.ClassSchedule.StartTime >= weekStart &&
                     b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookingCount >= maxPerWeek)
                throw new BusinessRuleException($"Weekly booking limit of {maxPerWeek} reached for your plan.");
        }

        // Rule 7: No double booking (overlapping classes)
        var hasOverlap = await _db.Bookings.AnyAsync(
            b => b.MemberId == request.MemberId &&
                 (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                 b.ClassSchedule.StartTime < schedule.EndTime &&
                 b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new ConflictException("You already have a booking for an overlapping class.");

        // Check existing booking for same class
        var existingBooking = await _db.Bookings.AnyAsync(
            b => b.MemberId == request.MemberId &&
                 b.ClassScheduleId == request.ClassScheduleId &&
                 (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking)
            throw new ConflictException("You already have an active booking for this class.");

        // Rule 2: Capacity management
        var isWaitlisted = schedule.CurrentEnrollment >= schedule.Capacity;

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
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

        schedule.UpdatedAt = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created booking {Id} for member {MemberId} - Status: {Status}",
            booking.Id, request.MemberId, booking.Status);

        return MapToResponse(booking, schedule, member);
    }

    public async Task<BookingResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await GetBookingWithRelations(id, ct);
        return MapToResponse(booking, booking.ClassSchedule, booking.Member);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await GetBookingWithRelations(id, ct);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new BusinessRuleException($"Cannot cancel a booking with status {booking.Status}.");

        var schedule = booking.ClassSchedule;
        var now = DateTime.UtcNow;

        // Rule 3: Cannot cancel after class started/completed
        if (schedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking after the class has started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var isLateCancellation = schedule.StartTime - now < TimeSpan.FromHours(2) && schedule.StartTime > now;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = isLateCancellation
            ? $"Late cancellation: {request.Reason ?? "No reason provided"}"
            : request.Reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Rule 2: Promote first waitlisted member
            var firstWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (firstWaitlisted is not null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = now;
                schedule.CurrentEnrollment++;
                schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                // Reorder remaining waitlist
                var remainingWaitlisted = await _db.Bookings
                    .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }
            }
        }
        else if (booking.Status == BookingStatus.Cancelled && booking.WaitlistPosition.HasValue)
        {
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);
        }

        schedule.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled booking {Id}{Late}", id, isLateCancellation ? " (late cancellation)" : "");
        return MapToResponse(booking, schedule, booking.Member);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await GetBookingWithRelations(id, ct);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var schedule = booking.ClassSchedule;

        // Rule 11: Check-in window - 15 min before to 15 min after class start
        var checkInWindowStart = schedule.StartTime.AddMinutes(-15);
        var checkInWindowEnd = schedule.StartTime.AddMinutes(15);

        if (now < checkInWindowStart || now > checkInWindowEnd)
            throw new BusinessRuleException("Check-in is only available 15 minutes before to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Checked in booking {Id}", id);
        return MapToResponse(booking, schedule, booking.Member);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await GetBookingWithRelations(id, ct);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        var schedule = booking.ClassSchedule;

        // Rule 12: Can mark no-show 15 min after class start
        if (now < schedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Can only mark no-show 15 minutes after class start.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Marked booking {Id} as no-show", id);
        return MapToResponse(booking, schedule, booking.Member);
    }

    private async Task<Booking> GetBookingWithRelations(int id, CancellationToken ct)
    {
        return await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");
    }

    private static (DateTime WeekStart, DateTime WeekEnd) GetIsoWeekRange(DateTime date)
    {
        var day = date.Date;
        var diff = (7 + (day.DayOfWeek - DayOfWeek.Monday)) % 7;
        var weekStart = day.AddDays(-diff);
        var weekEnd = weekStart.AddDays(7);
        return (weekStart, weekEnd);
    }

    private static BookingResponse MapToResponse(Booking b, ClassSchedule cs, Member m) => new(
        b.Id, b.ClassScheduleId, cs.ClassType.Name,
        b.MemberId, $"{m.FirstName} {m.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        cs.StartTime, cs.EndTime, cs.Room,
        b.CreatedAt, b.UpdatedAt);
}
