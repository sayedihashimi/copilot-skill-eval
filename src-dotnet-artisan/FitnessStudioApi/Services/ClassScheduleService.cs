using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? hasAvailability, PaginationParams pagination);
    Task<ClassScheduleResponse> GetByIdAsync(int id);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request);
    Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request);
    Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request);
    Task<IReadOnlyList<RosterEntry>> GetRosterAsync(int id);
    Task<IReadOnlyList<RosterEntry>> GetWaitlistAsync(int id);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync();
}

public class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? hasAvailability, PaginationParams pagination)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);
        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<ClassScheduleResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ClassScheduleResponse> GetByIdAsync(int id)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new NotFoundException($"Class schedule with ID {id} not found");

        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request)
    {
        var classType = await db.ClassTypes.FindAsync(request.ClassTypeId)
            ?? throw new NotFoundException($"Class type with ID {request.ClassTypeId} not found");

        var instructor = await db.Instructors.FindAsync(request.InstructorId)
            ?? throw new NotFoundException($"Instructor with ID {request.InstructorId} not found");

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict during this time");

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
        await db.SaveChangesAsync();
        logger.LogInformation("Scheduled class {ClassId}: {ClassName} at {StartTime}", schedule.Id, classType.Name, schedule.StartTime);

        // Reload with relations
        return await GetByIdAsync(schedule.Id);
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new NotFoundException($"Class schedule with ID {id} not found");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Only scheduled classes can be updated");

        // Check instructor conflicts (excluding this class)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict during this time");

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated class schedule: {ClassId}", id);
        return await GetByIdAsync(id);
    }

    public async Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new NotFoundException($"Class schedule with ID {id} not found");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Only scheduled classes can be cancelled");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason;
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

        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled class {ClassId} with reason: {Reason}", id, request.Reason);
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<RosterEntry>> GetRosterAsync(int id)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new NotFoundException($"Class schedule with ID {id} not found");

        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntry(b.Id, b.MemberId, b.Member.FirstName + " " + b.Member.LastName, b.BookingDate, b.Status.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<RosterEntry>> GetWaitlistAsync(int id)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new NotFoundException($"Class schedule with ID {id} not found");

        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new RosterEntry(b.Id, b.MemberId, b.Member.FirstName + " " + b.Member.LastName, b.BookingDate, b.Status.ToString()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var classes = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime >= now &&
                         cs.StartTime <= nextWeek &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync();

        return classes.Select(MapToResponse).ToList();
    }

    internal static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name, cs.InstructorId,
        $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status.ToString(), cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
