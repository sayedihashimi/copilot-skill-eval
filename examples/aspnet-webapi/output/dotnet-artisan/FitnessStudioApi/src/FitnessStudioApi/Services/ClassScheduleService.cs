using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleListResponse>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(cs => cs.StartTime <= toDate.Value);
        }

        if (classTypeId.HasValue)
        {
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(cs => cs.InstructorId == instructorId.Value);
        }

        if (hasAvailability == true)
        {
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => new ClassScheduleListResponse(
                cs.Id, cs.ClassType.Name,
                cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment, cs.Room, cs.Status))
            .ToListAsync(ct);

        return new PaginatedResponse<ClassScheduleListResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cs = await db.ClassSchedules
            .AsNoTracking()
            .Include(s => s.ClassType)
            .Include(s => s.Instructor)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (cs is null)
        {
            return null;
        }

        return MapToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (!instructor.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an inactive instructor.");
        }

        // Check instructor schedule conflict
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");
        }

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity ?? classType.DefaultCapacity,
            Room = request.Room,
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class schedule {ScheduleId} created for {ClassType} with instructor {Instructor}",
            schedule.Id, classType.Name, $"{instructor.FirstName} {instructor.LastName}");

        return (await GetByIdAsync(schedule.Id, ct))!;
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules.FindAsync([id], ct);
        if (schedule is null)
        {
            return null;
        }

        if (schedule.Status == ClassScheduleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot update a cancelled class.");
        }

        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        // Check instructor conflict (excluding this class)
        if (request.InstructorId != schedule.InstructorId ||
            request.StartTime != schedule.StartTime ||
            request.EndTime != schedule.EndTime)
        {
            var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
                cs.Id != id &&
                cs.InstructorId == request.InstructorId &&
                cs.Status != ClassScheduleStatus.Cancelled &&
                cs.StartTime < request.EndTime &&
                cs.EndTime > request.StartTime, ct);

            if (hasConflict)
            {
                throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");
            }
        }

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity ?? classType.DefaultCapacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
        {
            throw new InvalidOperationException("Class is already cancelled.");
        }

        if (schedule.Status == ClassScheduleStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed class.");
        }

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason ?? "Class cancelled by studio";
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all bookings for this class
        foreach (var booking in schedule.Bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Class {ScheduleId} cancelled. All bookings cancelled.", id);

        return MapToResponse(schedule);
    }

    public async Task<List<RosterEntryResponse>> GetRosterAsync(int classId, CancellationToken ct = default)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct))
        {
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");
        }

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryResponse(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.Member.Email, b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<List<WaitlistEntryResponse>> GetWaitlistAsync(int classId, CancellationToken ct = default)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct))
        {
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");
        }

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryResponse(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<List<ClassScheduleListResponse>> GetAvailableAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                cs.StartTime > now &&
                cs.StartTime <= weekFromNow &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleListResponse(
                cs.Id, cs.ClassType.Name,
                cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment, cs.Room, cs.Status))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment), cs.Room, cs.Status,
        cs.CancellationReason, cs.CreatedAt, cs.UpdatedAt);
}
