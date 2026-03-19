using System.Globalization;
using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class BookingService : IBookingService
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

        var schedule = await _context.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new KeyNotFoundException($"Class schedule with ID {dto.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Cannot book a class that is not in Scheduled status.");

        var now = DateTime.UtcNow;

        // Booking window: up to 7 days in advance, no less than 30 minutes before
        if (schedule.StartTime > now.AddDays(7))
            throw new InvalidOperationException("Cannot book classes more than 7 days in advance.");
        if (schedule.StartTime < now.AddMinutes(30))
            throw new InvalidOperationException("Cannot book a class less than 30 minutes before start time.");

        // Active membership required
        var activeMembership = await _context.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == dto.MemberId && ms.Status == MembershipStatus.Active)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Member does not have an active membership. Only active memberships can book classes.");

        // Premium class check
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new InvalidOperationException("Your membership plan does not allow booking premium classes. Please upgrade to Premium or Elite.");

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1) // -1 = unlimited
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await _context.Bookings
                .Where(b => b.MemberId == dto.MemberId
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended)
                    && b.BookingDate >= weekStart
                    && b.BookingDate < weekEnd)
                .CountAsync();

            if (weeklyBookingCount >= maxPerWeek)
                throw new InvalidOperationException($"Weekly booking limit reached ({maxPerWeek} per week). Upgrade your plan for more bookings.");
        }

        // No double booking (check for overlapping classes)
        var hasOverlap = await _context.Bookings
            .Where(b => b.MemberId == dto.MemberId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
            .Join(_context.ClassSchedules,
                b => b.ClassScheduleId,
                cs => cs.Id,
                (b, cs) => cs)
            .AnyAsync(cs => cs.StartTime < schedule.EndTime && cs.EndTime > schedule.StartTime);

        if (hasOverlap)
            throw new InvalidOperationException("You already have a booking for an overlapping class time.");

        // Create booking
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
            BookingDate = now
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

        schedule.UpdatedAt = now;
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Booking created (ID: {BookingId}) for member {MemberId}, class {ClassId}, status: {Status}",
            booking.Id, dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetDtoByIdAsync(booking.Id);
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking == null ? null : MapToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new InvalidOperationException("Cannot cancel a booking that has been attended.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.StartTime <= now)
            throw new InvalidOperationException("Cannot cancel a booking for a class that has already started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.UpdatedAt = now;

        // Determine late cancellation
        var hoursUntilClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        if (hoursUntilClass < 2)
        {
            booking.CancellationReason = dto.CancellationReason != null
                ? $"{dto.CancellationReason} (late cancellation)"
                : "Late cancellation (less than 2 hours before class)";
        }
        else
        {
            booking.CancellationReason = dto.CancellationReason;
        }

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first waitlisted member
            var nextWaitlisted = await _context.Bookings
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

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await _context.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                _logger.LogInformation("Promoted booking {BookingId} from waitlist to confirmed", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;
            booking.WaitlistPosition = null;

            // Reorder remaining waitlist
            var remainingWaitlisted = await _context.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cancelled booking (ID: {BookingId})", id);
        return MapToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 minutes before to 15 minutes after
        if (now < classStart.AddMinutes(-15))
            throw new InvalidOperationException("Check-in is not yet available. You can check in starting 15 minutes before class.");
        if (now > classStart.AddMinutes(15))
            throw new InvalidOperationException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Member {MemberId} checked in for booking {BookingId}", booking.MemberId, id);
        return MapToDto(booking);
    }

    public async Task<BookingDto> NoShowAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new InvalidOperationException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} marked as no-show", id);
        return MapToDto(booking);
    }

    private async Task<BookingDto> GetDtoByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstAsync(b => b.Id == id);
        return MapToDto(booking);
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? "",
        MemberId = b.MemberId,
        MemberName = b.Member != null ? $"{b.Member.FirstName} {b.Member.LastName}" : "",
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        ClassStartTime = b.ClassSchedule?.StartTime ?? DateTime.MinValue,
        ClassEndTime = b.ClassSchedule?.EndTime ?? DateTime.MinValue,
        Room = b.ClassSchedule?.Room ?? "",
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var day = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
        return date.Date.AddDays(-day);
    }
}
