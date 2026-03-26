using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs.Booking;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FitnessStudioApi.Services;

public sealed class BookingService : IBookingService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(FitnessDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        var member = await _context.Members.FindAsync(dto.MemberId)
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var classSchedule = await _context.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new KeyNotFoundException($"Class schedule with ID {dto.ClassScheduleId} not found.");

        if (classSchedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{classSchedule.Status}'.");

        // Rule 6: Active membership required
        var activeMembership = await _context.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.MemberId == dto.MemberId && m.Status == MembershipStatus.Active);

        if (activeMembership == null)
            throw new BusinessRuleException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Rule 4: Membership tier access
        if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException(
                $"Your '{activeMembership.MembershipPlan.Name}' plan does not allow booking premium classes. Please upgrade to a Premium or Elite plan.");

        // Rule 1: Booking window (7 days in advance, no less than 30 minutes before)
        var now = DateTime.UtcNow;
        var maxAdvanceDate = now.AddDays(7);
        var minBookingTime = classSchedule.StartTime.AddMinutes(-30);

        if (classSchedule.StartTime > maxAdvanceDate)
            throw new BusinessRuleException("Cannot book a class more than 7 days in advance.");

        if (now > minBookingTime)
            throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");

        // Rule 7: No double booking (overlapping classes)
        var hasOverlap = await _context.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == dto.MemberId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted)
                && b.ClassSchedule.StartTime < classSchedule.EndTime
                && b.ClassSchedule.EndTime > classSchedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for a class that overlaps with this time slot.");

        // Rule 5: Weekly booking limits
        var maxBookingsPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxBookingsPerWeek != -1) // -1 means unlimited
        {
            var isoWeek = ISOWeek.GetWeekOfYear(classSchedule.StartTime);
            var isoYear = ISOWeek.GetYear(classSchedule.StartTime);
            var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await _context.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b => b.MemberId == dto.MemberId
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended)
                    && b.ClassSchedule.StartTime >= weekStart
                    && b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookings >= maxBookingsPerWeek)
                throw new BusinessRuleException(
                    $"You have reached your weekly booking limit of {maxBookingsPerWeek} classes for your '{activeMembership.MembershipPlan.Name}' plan.");
        }

        // Check if already booked for this exact class
        var existingBooking = await _context.Bookings
            .AnyAsync(b => b.MemberId == dto.MemberId
                && b.ClassScheduleId == dto.ClassScheduleId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));

        if (existingBooking)
            throw new BusinessRuleException("You already have a booking for this class.");

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
            BookingDate = DateTime.UtcNow
        };

        if (classSchedule.CurrentEnrollment < classSchedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            classSchedule.CurrentEnrollment++;
        }
        else
        {
            // Waitlist
            var maxWaitlistPosition = await _context.Bookings
                .Where(b => b.ClassScheduleId == dto.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .MaxAsync(b => (int?)b.WaitlistPosition) ?? 0;

            booking.Status = BookingStatus.Waitlisted;
            booking.WaitlistPosition = maxWaitlistPosition + 1;
            classSchedule.WaitlistCount++;
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Booking created: Member {MemberId} booked class {ClassScheduleId} with status {Status}",
            dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id);
    }

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        return MapToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new BusinessRuleException("Cannot cancel a booking that has already been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;

        // Rule 3: Cannot cancel a class that has already started or completed
        if (booking.ClassSchedule.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking for a completed class.");

        if (booking.ClassSchedule.Status == ClassScheduleStatus.InProgress)
            throw new BusinessRuleException("Cannot cancel a booking for a class that is in progress.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        // Check late cancellation
        var hoursUntilClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        var reason = dto.Reason ?? string.Empty;
        if (hoursUntilClass < 2 && hoursUntilClass > 0)
        {
            reason = string.IsNullOrEmpty(reason)
                ? "Late cancellation (less than 2 hours before class)"
                : $"{reason} [Late cancellation]";
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;

        // Update class schedule counts
        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote from waitlist
            var nextWaitlisted = await _context.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                    && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Re-number remaining waitlist
                var remainingWaitlist = await _context.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                        && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlist.Count; i++)
                {
                    remainingWaitlist[i].WaitlistPosition = i + 1;
                }

                _logger.LogInformation("Promoted member {MemberId} from waitlist for class {ClassScheduleId}",
                    nextWaitlisted.MemberId, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Re-number remaining waitlist
            var remainingWaitlist = await _context.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                    && b.Status == BookingStatus.Waitlisted
                    && b.WaitlistPosition > booking.WaitlistPosition)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            foreach (var waitlisted in remainingWaitlist)
            {
                waitlisted.WaitlistPosition--;
            }

            booking.WaitlistPosition = null;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled booking {BookingId} for member {MemberId}", id, booking.MemberId);
        return MapToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Cannot check in a booking with status '{booking.Status}'. Only confirmed bookings can be checked in.");

        // Rule 11: Check-in window (15 min before to 15 min after class start)
        var now = DateTime.UtcNow;
        var checkInWindowStart = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var checkInWindowEnd = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < checkInWindowStart)
            throw new BusinessRuleException("Check-in is not available yet. You can check in starting 15 minutes before class.");

        if (now > checkInWindowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is only available up to 15 minutes after class start time.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Member {MemberId} checked in for class {ClassScheduleId}", booking.MemberId, booking.ClassScheduleId);
        return MapToDto(booking);
    }

    public async Task<BookingDto> MarkNoShowAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Cannot mark as no-show a booking with status '{booking.Status}'. Only confirmed bookings can be marked as no-show.");

        // Rule 12: Can only be flagged as no-show after 15 minutes past class start
        var now = DateTime.UtcNow;
        var noShowThreshold = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < noShowThreshold)
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked booking {BookingId} as no-show for member {MemberId}", id, booking.MemberId);
        return MapToDto(booking);
    }

    private static BookingDto MapToDto(Booking booking) => new()
    {
        Id = booking.Id,
        ClassScheduleId = booking.ClassScheduleId,
        ClassName = booking.ClassSchedule.ClassType.Name,
        MemberId = booking.MemberId,
        MemberName = $"{booking.Member.FirstName} {booking.Member.LastName}",
        BookingDate = booking.BookingDate,
        Status = booking.Status.ToString(),
        WaitlistPosition = booking.WaitlistPosition,
        CheckInTime = booking.CheckInTime,
        CancellationDate = booking.CancellationDate,
        CancellationReason = booking.CancellationReason,
        ClassStartTime = booking.ClassSchedule.StartTime,
        ClassEndTime = booking.ClassSchedule.EndTime,
        Room = booking.ClassSchedule.Room,
        InstructorName = $"{booking.ClassSchedule.Instructor.FirstName} {booking.ClassSchedule.Instructor.LastName}",
        CreatedAt = booking.CreatedAt,
        UpdatedAt = booking.UpdatedAt
    };
}
