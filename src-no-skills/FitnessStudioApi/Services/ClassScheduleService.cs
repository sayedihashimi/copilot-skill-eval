using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class ClassScheduleService : IClassScheduleService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext context, ILogger<ClassScheduleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ClassScheduleDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? available, PaginationParams pagination)
    {
        var query = _context.ClassSchedules
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
        if (available == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(cs => MapToDto(cs))
            .ToListAsync();

        return new PagedResult<ClassScheduleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(int id)
    {
        var cs = await _context.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cs == null ? null : MapToDto(cs);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _context.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new KeyNotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        var instructor = await _context.Instructors.FindAsync(dto.InstructorId)
            ?? throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        var duration = dto.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = dto.Capacity ?? classType.DefaultCapacity;
        var endTime = dto.StartTime.AddMinutes(duration);

        // Check instructor schedule conflicts
        var hasConflict = await _context.ClassSchedules
            .Where(cs => cs.InstructorId == dto.InstructorId
                && cs.Status != ClassScheduleStatus.Cancelled
                && cs.StartTime < endTime
                && cs.EndTime > dto.StartTime)
            .AnyAsync();

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = dto.Room
        };

        _context.ClassSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Scheduled class {ClassName} (ID: {ScheduleId}) with instructor {InstructorName}",
            classType.Name, schedule.Id, $"{instructor.FirstName} {instructor.LastName}");

        return (await GetByIdAsync(schedule.Id))!;
    }

    public async Task<ClassScheduleDto?> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await _context.ClassSchedules.FindAsync(id);
        if (schedule == null) return null;

        if (!await _context.ClassTypes.AnyAsync(ct => ct.Id == dto.ClassTypeId))
            throw new KeyNotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        if (!await _context.Instructors.AnyAsync(i => i.Id == dto.InstructorId))
            throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        // Check instructor conflict (exclude self)
        var hasConflict = await _context.ClassSchedules
            .Where(cs => cs.Id != id
                && cs.InstructorId == dto.InstructorId
                && cs.Status != ClassScheduleStatus.Cancelled
                && cs.StartTime < dto.EndTime
                && cs.EndTime > dto.StartTime)
            .AnyAsync();

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");

        schedule.ClassTypeId = dto.ClassTypeId;
        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _context.ClassSchedules
            .Include(cs => cs.Bookings)
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.CancellationReason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cascade cancel all bookings
        foreach (var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cancelled class schedule (ID: {ScheduleId}), all bookings cancelled", id);
        return MapToDto(schedule);
    }

    public async Task<List<ClassRosterItemDto>> GetRosterAsync(int id)
    {
        if (!await _context.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _context.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterItemDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Status = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<ClassRosterItemDto>> GetWaitlistAsync(int id)
    {
        if (!await _context.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _context.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassRosterItemDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Status = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<ClassScheduleDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await _context.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled
                && cs.StartTime >= now
                && cs.StartTime <= weekFromNow
                && cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToDto(cs))
            .ToListAsync();
    }

    private static ClassScheduleDto MapToDto(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType?.Name ?? "",
        InstructorId = cs.InstructorId,
        InstructorName = cs.Instructor != null ? $"{cs.Instructor.FirstName} {cs.Instructor.LastName}" : "",
        StartTime = cs.StartTime,
        EndTime = cs.EndTime,
        Capacity = cs.Capacity,
        CurrentEnrollment = cs.CurrentEnrollment,
        WaitlistCount = cs.WaitlistCount,
        Room = cs.Room,
        Status = cs.Status,
        CancellationReason = cs.CancellationReason,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
