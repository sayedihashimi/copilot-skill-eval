using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.DTOs.ClassSchedule;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService : IClassScheduleService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext context, ILogger<ClassScheduleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ClassScheduleDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page = 1, int pageSize = 10)
    {
        var query = _context.ClassSchedules.AsNoTracking()
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
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToDto(cs))
            .ToListAsync();

        return new PaginatedResponse<ClassScheduleDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ClassScheduleDto> GetByIdAsync(int id)
    {
        var schedule = await _context.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return MapToDto(schedule);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _context.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new KeyNotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        var instructor = await _context.Instructors.FindAsync(dto.InstructorId)
            ?? throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign a class to an inactive instructor.");

        var duration = dto.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = dto.Capacity ?? classType.DefaultCapacity;
        var endTime = dto.StartTime.AddMinutes(duration);

        // Check for instructor schedule conflicts
        var hasConflict = await _context.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == dto.InstructorId
                && cs.Status != ClassScheduleStatus.Cancelled
                && cs.StartTime < endTime
                && cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict during this time.", 409, "Schedule Conflict");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = dto.Room,
            Status = ClassScheduleStatus.Scheduled
        };

        _context.ClassSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Scheduled class: {ClassName} with {Instructor} at {StartTime} (ID: {ScheduleId})",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", dto.StartTime, schedule.Id);

        return await GetByIdAsync(schedule.Id);
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await _context.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Cannot update a cancelled class.");

        if (dto.InstructorId.HasValue)
        {
            var instructor = await _context.Instructors.FindAsync(dto.InstructorId.Value)
                ?? throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId.Value} not found.");

            if (!instructor.IsActive)
                throw new BusinessRuleException("Cannot assign a class to an inactive instructor.");
        }

        var newStartTime = dto.StartTime ?? schedule.StartTime;
        var duration = dto.DurationMinutes ?? (schedule.EndTime - schedule.StartTime).TotalMinutes;
        var newEndTime = newStartTime.AddMinutes(duration);
        var newInstructorId = dto.InstructorId ?? schedule.InstructorId;

        // Check instructor conflicts (exclude current schedule)
        if (dto.InstructorId.HasValue || dto.StartTime.HasValue)
        {
            var hasConflict = await _context.ClassSchedules
                .AnyAsync(cs => cs.Id != id
                    && cs.InstructorId == newInstructorId
                    && cs.Status != ClassScheduleStatus.Cancelled
                    && cs.StartTime < newEndTime
                    && cs.EndTime > newStartTime);

            if (hasConflict)
                throw new BusinessRuleException("Instructor has a scheduling conflict during this time.", 409, "Schedule Conflict");
        }

        if (dto.InstructorId.HasValue) schedule.InstructorId = dto.InstructorId.Value;
        if (dto.StartTime.HasValue)
        {
            schedule.StartTime = newStartTime;
            schedule.EndTime = newEndTime;
        }
        if (dto.Capacity.HasValue) schedule.Capacity = dto.Capacity.Value;
        if (dto.Room != null) schedule.Room = dto.Room;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated class schedule {ScheduleId}", id);
        return await GetByIdAsync(id);
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
            throw new BusinessRuleException("Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.Reason ?? "Class cancelled by studio";

        // Cancel all bookings for this class
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled class schedule {ScheduleId} with {BookingCount} bookings cancelled",
            id, schedule.Bookings.Count);

        return MapToDto(schedule);
    }

    public async Task<List<RosterEntryDto>> GetRosterAsync(int id)
    {
        if (!await _context.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _context.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
                BookingStatus = b.Status.ToString(),
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<RosterEntryDto>> GetWaitlistAsync(int id)
    {
        if (!await _context.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _context.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new RosterEntryDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
                BookingStatus = b.Status.ToString(),
                BookingDate = b.BookingDate,
                CheckInTime = null
            })
            .ToListAsync();
    }

    public async Task<List<ClassScheduleDto>> GetAvailableClassesAsync()
    {
        var now = DateTime.UtcNow;
        var sevenDaysFromNow = now.AddDays(7);

        return await _context.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled
                && cs.StartTime >= now
                && cs.StartTime <= sevenDaysFromNow
                && cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToDto(cs))
            .ToListAsync();
    }

    private static ClassScheduleDto MapToDto(ClassSchedule cs) => new()
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
        Status = cs.Status.ToString(),
        CancellationReason = cs.CancellationReason,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
