using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger)
    : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity &&
                                      cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);
        var schedules = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = schedules.Select(MapToResponse).ToList();
        return PaginatedResponse<ClassScheduleResponse>.Create(items, page, pageSize, totalCount);
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
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new ArgumentException("Cannot assign an inactive instructor to a class.");

        var durationMinutes = request.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = request.Capacity ?? classType.DefaultCapacity;
        var endTime = request.StartTime.AddMinutes(durationMinutes);

        // Check instructor schedule conflicts
        var instructorConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < endTime &&
                           cs.EndTime > request.StartTime, ct);

        if (instructorConflict)
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = request.Room,
            Status = ClassScheduleStatus.Scheduled
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        await db.Entry(schedule).Reference(cs => cs.ClassType).LoadAsync(ct);
        await db.Entry(schedule).Reference(cs => cs.Instructor).LoadAsync(ct);

        logger.LogInformation("Scheduled class {ClassTypeName} with instructor {InstructorName} at {StartTime}",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", schedule.StartTime);

        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Can only update scheduled classes.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        var durationMinutes = request.DurationMinutes ?? schedule.ClassType.DefaultDurationMinutes;
        var endTime = request.StartTime.AddMinutes(durationMinutes);

        // Check instructor schedule conflicts (excluding this class)
        var instructorConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Id != id &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < endTime &&
                           cs.EndTime > request.StartTime, ct);

        if (instructorConflict)
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = endTime;
        if (request.Capacity.HasValue)
            schedule.Capacity = request.Capacity.Value;
        schedule.Room = request.Room;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated class schedule {ScheduleId}", schedule.Id);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Can only cancel scheduled classes.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.CancellationReason;

        // Cancel all bookings for this class
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled class schedule {ScheduleId}. All bookings cancelled.", schedule.Id);
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int classId, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterEntry(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassWaitlistEntry>> GetWaitlistAsync(int classId, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassWaitlistEntry(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.WaitlistPosition ?? 0, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        return await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                        cs.StartTime > now &&
                        cs.StartTime <= nextWeek &&
                        cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) =>
        new(cs.Id, cs.ClassTypeId, cs.ClassType.Name,
            cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity,
            cs.CurrentEnrollment, cs.WaitlistCount,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
            cs.Room, cs.Status, cs.CancellationReason,
            cs.CreatedAt, cs.UpdatedAt);
}
