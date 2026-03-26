using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;
using System.Globalization;

namespace FitnessStudioApi.Services;

public class BookingService : IBookingService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new BusinessRuleException("Member not found.", 404, "Not Found");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new BusinessRuleException("Class schedule not found.", 404, "Not Found");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("This class is not available for booking.");

        // Check booking window (up to 7 days in advance, no less than 30 min before start)
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");
        if (schedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book classes less than 30 minutes before start time.");

        // Check active membership
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == dto.MemberId && ms.Status == MembershipStatus.Active);

        if (activeMembership == null)
            throw new BusinessRuleException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Check premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow booking premium classes. Please upgrade to Premium or Elite.");

        // Check weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b => b.MemberId == dto.MemberId &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                               b.ClassSchedule.StartTime >= weekStart && b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookings >= maxPerWeek)
                throw new BusinessRuleException($"Weekly booking limit reached ({maxPerWeek} per week for your plan).");
        }

        // Check no double booking (overlapping classes)
        var overlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == dto.MemberId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                          b.ClassSchedule.StartTime < schedule.EndTime &&
                          b.ClassSchedule.EndTime > schedule.StartTime);

        if (overlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class.");

        // Check if already booked for this specific class
        var existingBooking = await _db.Bookings.AnyAsync(b =>
            b.MemberId == dto.MemberId &&
            b.ClassScheduleId == dto.ClassScheduleId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));

        if (existingBooking)
            throw new BusinessRuleException("You already have a booking for this class.");

        // Create booking
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId
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
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking created: Member {MemberId} for class {ClassId}, status: {Status}",
            dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdInternalAsync(booking.Id);
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking == null ? null : BookingServiceHelper.ToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404, "Not Found");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new BusinessRuleException("Cannot cancel a booking that has been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.StartTime <= now)
            throw new BusinessRuleException("Cannot cancel a class that has already started or completed.");

        // Late cancellation check
        var reason = dto.CancellationReason;
        var hoursBeforeClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        if (hoursBeforeClass < 2)
        {
            reason = (reason ?? "") + " [Late cancellation - less than 2 hours before class]";
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first from waitlist
            var nextWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist
                var remaining = await _db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remaining.Count; i++)
                {
                    remaining[i].WaitlistPosition = i + 1;
                }

                _logger.LogInformation("Waitlist promotion: Booking {BookingId} promoted to confirmed", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;
            booking.WaitlistPosition = null;

            // Reorder waitlist
            var remaining = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].WaitlistPosition = i + 1;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking {Id} cancelled", id);
        return BookingServiceHelper.ToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404, "Not Found");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 min before to 15 min after start
        if (now < classStart.AddMinutes(-15))
            throw new BusinessRuleException("Check-in is not yet available. Check-in opens 15 minutes before class start.");

        if (now > classStart.AddMinutes(15))
            throw new BusinessRuleException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Member {MemberId} checked in for class {ClassId}", booking.MemberId, booking.ClassScheduleId);

        return BookingServiceHelper.ToDto(booking);
    }

    public async Task<BookingDto> NoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404, "Not Found");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Booking {Id} marked as no-show", id);

        return BookingServiceHelper.ToDto(booking);
    }

    private async Task<BookingDto> GetByIdInternalAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstAsync(b => b.Id == id);

        return BookingServiceHelper.ToDto(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        // ISO week starts on Monday (1), Sunday is 0 -> treat as 7
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.Date.AddDays(-daysToMonday);
    }
}
