using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleListDto>> GetAllAsync(
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
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => new ClassScheduleListDto(
                cs.Id, cs.ClassType.Name,
                $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
                cs.StartTime, cs.EndTime,
                cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment,
                cs.Room, cs.Status))
            .ToListAsync(ct);

        return new PaginatedResponse<ClassScheduleListDto>(items, totalCount, page, pageSize);
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await db.ClassSchedules
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return cs is null ? null : MapToDto(cs);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([dto.ClassTypeId], ct)
            ?? throw new NotFoundException($"Class type with Id {dto.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([dto.InstructorId], ct)
            ?? throw new NotFoundException($"Instructor with Id {dto.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor to a class.");

        var durationMinutes = dto.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = dto.Capacity ?? classType.DefaultCapacity;
        var endTime = dto.StartTime.AddMinutes(durationMinutes);

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == dto.InstructorId &&
                cs.Status != ClassStatus.Cancelled &&
                cs.StartTime < endTime && cs.EndTime > dto.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict at the requested time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = dto.Room
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Scheduled class {ClassTypeName} with Id {ClassId} at {StartTime}",
            classType.Name, schedule.Id, schedule.StartTime);

        return (await GetByIdAsync(schedule.Id, ct))!;
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new NotFoundException($"Class schedule with Id {id} not found.");

        if (schedule.Status is ClassStatus.Cancelled or ClassStatus.Completed)
            throw new BusinessRuleException($"Cannot update a class with status '{schedule.Status}'.");

        var instructor = await db.Instructors.FindAsync([dto.InstructorId], ct)
            ?? throw new NotFoundException($"Instructor with Id {dto.InstructorId} not found.");

        var durationMinutes = dto.DurationMinutes ?? schedule.ClassType.DefaultDurationMinutes;
        var endTime = dto.StartTime.AddMinutes(durationMinutes);

        // Check instructor conflicts (excluding current schedule)
        var hasConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == dto.InstructorId &&
                cs.Id != id &&
                cs.Status != ClassStatus.Cancelled &&
                cs.StartTime < endTime && cs.EndTime > dto.StartTime, ct);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict at the requested time.");

        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = endTime;
        if (dto.Capacity.HasValue)
            schedule.Capacity = dto.Capacity.Value;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated class schedule {ClassId}", id);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new NotFoundException($"Class schedule with Id {id} not found.");

        if (schedule.Status == ClassStatus.Cancelled)
            throw new BusinessRuleException("This class is already cancelled.");

        if (schedule.Status == ClassStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a completed class.");

        schedule.Status = ClassStatus.Cancelled;
        schedule.CancellationReason = dto.CancellationReason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
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
        logger.LogInformation("Cancelled class {ClassId} and all associated bookings", id);

        return MapToDto(schedule);
    }

    public async Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new NotFoundException($"Class schedule with Id {id} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryDto(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.Status.ToString(), b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new NotFoundException($"Class schedule with Id {id} not found.");

        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryDto(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleListDto>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sevenDaysOut = now.AddDays(7);

        return await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassStatus.Scheduled &&
                cs.StartTime > now && cs.StartTime <= sevenDaysOut &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleListDto(
                cs.Id, cs.ClassType.Name,
                $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
                cs.StartTime, cs.EndTime,
                cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment,
                cs.Room, cs.Status))
            .ToListAsync(ct);
    }

    private static ClassScheduleDto MapToDto(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId,
        $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime,
        cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        cs.Capacity - cs.CurrentEnrollment,
        cs.Room, cs.Status, cs.CancellationReason);
}
