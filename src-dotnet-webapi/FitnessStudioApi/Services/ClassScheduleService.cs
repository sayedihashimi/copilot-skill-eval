using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassScheduleService(FitnessDbContext db) : IClassScheduleService
{
    public async Task<PagedResponse<ClassScheduleResponse>> GetAllAsync(
        DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(cs => cs.StartTime >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(cs => cs.StartTime <= to);
        }

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return new PagedResponse<ClassScheduleResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await db.ClassSchedules.AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return cs is null ? null : MapToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassTypeId, ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.InstructorId, ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new ArgumentException("Cannot schedule a class with an inactive instructor.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AsNoTracking()
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < request.EndTime &&
                           cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict during this time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            CurrentEnrollment = 0,
            WaitlistCount = 0,
            Room = request.Room,
            Status = ClassScheduleStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(schedule.Id, ct))!;
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled class.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot update a completed class.");

        var _ = await db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassTypeId, ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.InstructorId, ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflicts (excluding this class)
        var hasConflict = await db.ClassSchedules.AsNoTracking()
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Id != id &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < request.EndTime &&
                           cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict during this time.");

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(schedule.Id, ct))!;
    }

    public async Task<ClassScheduleResponse> CancelClassAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.CancellationReason;
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

        await db.SaveChangesAsync(ct);

        return MapToResponse(schedule);
    }

    public async Task<List<ClassRosterResponse>> GetRosterAsync(int classId, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AsNoTracking().AnyAsync(cs => cs.Id == classId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterResponse
            {
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Email = b.Member.Email,
                BookingStatus = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync(ct);
    }

    public async Task<List<ClassRosterResponse>> GetWaitlistAsync(int classId, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AsNoTracking().AnyAsync(cs => cs.Id == classId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {classId} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassRosterResponse
            {
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Email = b.Member.Email,
                BookingStatus = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync(ct);
    }

    public async Task<List<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sevenDays = now.AddDays(7);

        return await db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                        cs.StartTime > now &&
                        cs.StartTime <= sevenDays &&
                        cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType.Name,
        InstructorId = cs.InstructorId,
        InstructorName = $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        StartTime = cs.StartTime,
        EndTime = cs.EndTime,
        Capacity = cs.Capacity,
        CurrentEnrollment = cs.CurrentEnrollment,
        WaitlistCount = cs.WaitlistCount,
        Room = cs.Room,
        Status = cs.Status,
        CancellationReason = cs.CancellationReason,
        IsPremium = cs.ClassType.IsPremium,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
