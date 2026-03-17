using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<ClassScheduleService> _logger = logger;

    public async Task<PagedResult<ClassScheduleResponse>> GetAllAsync(DateTime? date, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(cs => cs.StartTime >= dayStart && cs.StartTime < dayEnd);
        }

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return new PagedResult<ClassScheduleResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ClassScheduleResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await _db.ClassSchedules
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return MapToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await _db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Rule 8: Instructor schedule conflict
        var instructorConflict = await _db.ClassSchedules.AnyAsync(
            cs => cs.InstructorId == request.InstructorId &&
                  cs.Status != ClassScheduleStatus.Cancelled &&
                  cs.StartTime < request.EndTime &&
                  cs.EndTime > request.StartTime, ct);

        if (instructorConflict)
            throw new ConflictException("Instructor already has a class scheduled during this time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            Room = request.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await _db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        _logger.LogInformation("Scheduled class {ClassName} with ID {Id}", classType.Name, schedule.Id);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Can only update scheduled classes.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        var instructorConflict = await _db.ClassSchedules.AnyAsync(
            cs => cs.InstructorId == request.InstructorId &&
                  cs.Id != id &&
                  cs.Status != ClassScheduleStatus.Cancelled &&
                  cs.StartTime < request.EndTime &&
                  cs.EndTime > request.StartTime, ct);

        if (instructorConflict)
            throw new ConflictException("Instructor already has a class scheduled during this time.");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status is ClassScheduleStatus.Cancelled or ClassScheduleStatus.Completed)
            throw new BusinessRuleException($"Cannot cancel a class that is already {schedule.Status}.");

        // Rule 10: Cancel all bookings
        foreach (var booking in schedule.Bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason ?? "Cancelled by studio";
        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled class {Id}, all bookings cancelled", id);
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<RosterEntryResponse>> GetRosterAsync(int id, CancellationToken ct)
    {
        var exists = await _db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists) throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryResponse(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.Member.Email, b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RosterEntryResponse>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        var exists = await _db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists) throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new RosterEntryResponse(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.Member.Email, b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await _db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.StartTime <= weekFromNow &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status, cs.CancellationReason,
        cs.ClassType.IsPremium, cs.CreatedAt, cs.UpdatedAt);
}
