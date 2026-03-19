using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService : IClassScheduleService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(cs => cs.StartTime >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(cs => cs.StartTime <= toDate.Value);
        if (classTypeId.HasValue) query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue) query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (hasAvailability == true) query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity,
                cs.CurrentEnrollment, cs.WaitlistCount,
                Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
                cs.Room, cs.Status.ToString(), cs.CancellationReason,
                cs.CreatedAt, cs.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<ClassScheduleResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await _db.ClassSchedules.AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return cs is null ? null : ToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type with id {request.ClassTypeId} not found.");

        var instructor = await _db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with id {request.InstructorId} not found.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict with another class at this time.", 409);

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity ?? classType.DefaultCapacity,
            Room = request.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await _db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        _logger.LogInformation("Class scheduled: {ClassType} with {Instructor} on {StartTime}",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", schedule.StartTime);

        return ToResponse(schedule);
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null) return null;

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Cannot update a cancelled class.");

        if (request.EndTime <= request.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor conflicts (excluding this class)
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict with another class at this time.", 409);

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        if (request.Capacity.HasValue) schedule.Capacity = request.Capacity.Value;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Reload navigation properties
        await _db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        return ToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with id {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Class {ClassId} cancelled. Reason: {Reason}. All bookings cancelled.", id, request.Reason);
        return ToResponse(schedule);
    }

    public async Task<IReadOnlyList<RosterEntryResponse>> GetRosterAsync(int classId, CancellationToken ct)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct))
            throw new KeyNotFoundException($"Class schedule with id {classId} not found.");

        return await _db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryResponse(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.Status.ToString(), b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WaitlistEntryResponse>> GetWaitlistAsync(int classId, CancellationToken ct)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == classId, ct))
            throw new KeyNotFoundException($"Class schedule with id {classId} not found.");

        return await _db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryResponse(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await _db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                cs.StartTime > now && cs.StartTime <= weekFromNow &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity,
                cs.CurrentEnrollment, cs.WaitlistCount,
                Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
                cs.Room, cs.Status.ToString(), cs.CancellationReason,
                cs.CreatedAt, cs.UpdatedAt))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse ToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
        cs.StartTime, cs.EndTime, cs.Capacity,
        cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status.ToString(), cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
