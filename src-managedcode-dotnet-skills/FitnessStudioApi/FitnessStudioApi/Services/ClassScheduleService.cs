using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PagedResult<ClassScheduleResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .OrderBy(cs => cs.StartTime);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return new PagedResult<ClassScheduleResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        return schedule is null ? null : MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new BusinessRuleException("Class type not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new BusinessRuleException("Instructor not found.");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts (Rule 8)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at this time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            Room = request.Room
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        await db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        logger.LogInformation("Scheduled class {ScheduleId}: {ClassName} at {StartTime}",
            schedule.Id, classType.Name, schedule.StartTime);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null) return null;

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Can only update scheduled classes.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts (exclude self)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Id != id &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at this time.");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        // Reload instructor in case it changed
        await db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        logger.LogInformation("Updated class schedule {ScheduleId}", schedule.Id);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest? request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new BusinessRuleException("Class schedule not found.");

        if (schedule.Status is ClassScheduleStatus.Cancelled or ClassScheduleStatus.Completed)
            throw new BusinessRuleException($"Cannot cancel a class with status '{schedule.Status}'.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request?.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cascade cancel all bookings (Rule 10)
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled class {ScheduleId} with reason: {Reason}",
            schedule.Id, request?.Reason ?? "No reason provided");
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new BusinessRuleException("Class schedule not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status != BookingStatus.Waitlisted)
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterEntry(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WaitlistEntry>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new BusinessRuleException("Class schedule not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntry(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
        cs.WaitlistCount, cs.Room, cs.Status, cs.CancellationReason,
        cs.ClassType.IsPremium,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.CreatedAt, cs.UpdatedAt);
}
