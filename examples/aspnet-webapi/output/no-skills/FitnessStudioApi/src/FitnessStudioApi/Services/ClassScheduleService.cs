using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class ClassScheduleService : IClassScheduleService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResult<ClassScheduleListDto>> GetAllAsync(int page, int pageSize, DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available)
    {
        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);
        if (classTypeId.HasValue) query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue) query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (available == true) query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var total = await query.CountAsync();
        var items = await query.OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(cs => new ClassScheduleListDto(
            cs.Id, cs.ClassType.Name, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment), cs.Room, cs.Status.ToString()));

        return new PaginatedResult<ClassScheduleListDto>(dtos, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(int id)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cs == null ? null : ToDetailDto(cs);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new BusinessRuleException("Class type not found.", 404, "Not Found");

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new BusinessRuleException("Instructor not found.", 404, "Not Found");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Instructor is not active.");

        if (dto.StartTime >= dto.EndTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor conflicts
        var conflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime && cs.EndTime > dto.StartTime);

        if (conflict)
            throw new BusinessRuleException("Instructor has a schedule conflict during this time.", 409, "Conflict");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity ?? classType.DefaultCapacity,
            Room = dto.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Class scheduled: {ClassType} at {Time} in {Room}", classType.Name, dto.StartTime, dto.Room);

        return (await GetByIdAsync(schedule.Id))!;
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new BusinessRuleException("Class schedule not found.", 404, "Not Found");

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new BusinessRuleException("Instructor not found.", 404, "Not Found");

        if (dto.StartTime >= dto.EndTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor conflicts (excluding this schedule)
        var conflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime && cs.EndTime > dto.StartTime);

        if (conflict)
            throw new BusinessRuleException("Instructor has a schedule conflict during this time.", 409, "Conflict");

        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(schedule.Id))!;
    }

    public async Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.Bookings)
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new BusinessRuleException("Class schedule not found.", 404, "Not Found");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.CancellationReason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all bookings for this class
        foreach (var booking in schedule.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Class {Id} cancelled. All bookings cancelled.", id);

        return ToDetailDto(schedule);
    }

    public async Task<IEnumerable<RosterEntryDto>> GetRosterAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new BusinessRuleException("Class schedule not found.", 404, "Not Found");

        var bookings = await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(b => new RosterEntryDto(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status.ToString()));
    }

    public async Task<IEnumerable<WaitlistEntryDto>> GetWaitlistAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new BusinessRuleException("Class schedule not found.", 404, "Not Found");

        var bookings = await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync();

        return bookings.Select(b => new WaitlistEntryDto(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.WaitlistPosition ?? 0, b.BookingDate));
    }

    public async Task<IEnumerable<ClassScheduleListDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        var schedules = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                        cs.StartTime >= now && cs.StartTime <= weekFromNow &&
                        cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync();

        return schedules.Select(cs => new ClassScheduleListDto(
            cs.Id, cs.ClassType.Name, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
            cs.Capacity - cs.CurrentEnrollment, cs.Room, cs.Status.ToString()));
    }

    private static ClassScheduleDto ToDetailDto(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name, cs.InstructorId,
        $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
        cs.WaitlistCount, Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status.ToString(), cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
