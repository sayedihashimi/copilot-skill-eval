using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class BookingService(FitnessDbContext db) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Inactive members cannot book classes.");

        var schedule = await db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new ArgumentException("Can only book classes with 'Scheduled' status.");

        var now = DateTime.UtcNow;

        // Booking window: 7 days ahead, 30 min before start
        if (schedule.StartTime > now.AddDays(7))
            throw new ArgumentException("Cannot book classes more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new ArgumentException("Cannot book classes less than 30 minutes before start.");

        // Active membership required
        var activeMembership = await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == request.MemberId &&
                                      ms.Status == MembershipStatus.Active, ct)
            ?? throw new ArgumentException("Member must have an active membership to book classes.");

        // Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new ArgumentException("Your membership plan does not allow premium class bookings. Upgrade to Premium or Elite.");

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await db.Bookings.AsNoTracking()
                .CountAsync(b => b.MemberId == request.MemberId &&
                                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                                b.ClassSchedule.StartTime >= weekStart &&
                                b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookings >= maxPerWeek)
                throw new ArgumentException($"Weekly booking limit of {maxPerWeek} reached for your membership plan.");
        }

        // No double booking: check existing booking for same class
        var alreadyBooked = await db.Bookings.AsNoTracking()
            .AnyAsync(b => b.MemberId == request.MemberId &&
                          b.ClassScheduleId == request.ClassScheduleId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (alreadyBooked)
            throw new InvalidOperationException("Member already has a booking for this class.");

        // No overlapping classes
        var hasOverlap = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == request.MemberId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                          b.ClassSchedule.StartTime < schedule.EndTime &&
                          b.ClassSchedule.EndTime > schedule.StartTime &&
                          b.ClassScheduleId != request.ClassScheduleId, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Member has an overlapping class booking during this time.");

        // Determine if confirmed or waitlisted
        var isWaitlisted = schedule.CurrentEnrollment >= schedule.Capacity;

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            BookingDate = now,
            Status = isWaitlisted ? BookingStatus.Waitlisted : BookingStatus.Confirmed,
            WaitlistPosition = isWaitlisted ? schedule.WaitlistCount + 1 : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Bookings.Add(booking);

        // Update schedule counters
        var scheduleEntity = await db.ClassSchedules.FindAsync([request.ClassScheduleId], ct);
        if (scheduleEntity != null)
        {
            if (isWaitlisted)
                scheduleEntity.WaitlistCount++;
            else
                scheduleEntity.CurrentEnrollment++;

            scheduleEntity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(booking.Id, ct))!;
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new InvalidOperationException("Cannot cancel a booking that has been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new InvalidOperationException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;

        // Cannot cancel started or completed class
        if (booking.ClassSchedule.Status == ClassScheduleStatus.InProgress)
            throw new InvalidOperationException("Cannot cancel booking for a class that has started.");

        if (booking.ClassSchedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel booking for a completed class.");

        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;
        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = request.CancellationReason;

        // Check if late cancellation (< 2 hours before class)
        if (booking.ClassSchedule.StartTime <= now.AddHours(2) && booking.ClassSchedule.StartTime > now)
        {
            booking.CancellationReason = (request.CancellationReason ?? "") + " [Late cancellation]";
        }

        booking.UpdatedAt = now;

        // Update schedule counters
        var schedule = await db.ClassSchedules.FindAsync([booking.ClassScheduleId], ct);
        if (schedule != null)
        {
            if (wasConfirmed)
            {
                schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

                // Auto-promote first waitlisted booking
                var nextWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                               b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .FirstOrDefaultAsync(ct);

                if (nextWaitlisted != null)
                {
                    nextWaitlisted.Status = BookingStatus.Confirmed;
                    nextWaitlisted.WaitlistPosition = null;
                    nextWaitlisted.UpdatedAt = now;

                    schedule.CurrentEnrollment++;
                    schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                    // Re-number remaining waitlist positions
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
                }
            }
            else if (wasWaitlisted)
            {
                schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                // Re-number remaining waitlist positions
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

            schedule.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 min before to 15 min after
        if (now < classStart.AddMinutes(-15))
            throw new ArgumentException("Check-in is not open yet. Check-in opens 15 minutes before class starts.");

        if (now > classStart.AddMinutes(15))
            throw new ArgumentException("Check-in window has closed. Check-in closes 15 minutes after class starts.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Can only mark no-show after 15 min past class start
        if (now < classStart.AddMinutes(15))
            throw new ArgumentException("Cannot mark no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return MapToResponse(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var diff = (dayOfWeek == 0 ? 6 : dayOfWeek - 1); // Monday = 0
        return date.Date.AddDays(-diff);
    }

    private static BookingResponse MapToResponse(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassTypeName = b.ClassSchedule.ClassType.Name,
        InstructorName = $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        ClassStartTime = b.ClassSchedule.StartTime,
        ClassEndTime = b.ClassSchedule.EndTime,
        Room = b.ClassSchedule.Room,
        MemberId = b.MemberId,
        MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
