using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class InstructorService : IInstructorService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<InstructorService> _logger;

    public InstructorService(FitnessDbContext context, ILogger<InstructorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<InstructorDto>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = _context.Instructors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var s = specialization.ToLower();
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(s));
        }

        return await query.OrderBy(i => i.LastName).Select(i => MapToDto(i)).ToListAsync();
    }

    public async Task<InstructorDto?> GetByIdAsync(int id)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        return instructor == null ? null : MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _context.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new InvalidOperationException($"An instructor with email '{dto.Email}' already exists.");

        var instructor = new Instructor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Bio = dto.Bio,
            Specializations = dto.Specializations,
            HireDate = dto.HireDate
        };

        _context.Instructors.Add(instructor);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created instructor: {Name} (ID: {InstructorId})", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<InstructorDto?> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return null;

        if (await _context.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new InvalidOperationException($"An instructor with email '{dto.Email}' already exists.");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(instructor);
    }

    public async Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _context.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = _context.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        return await query.OrderBy(cs => cs.StartTime)
            .Select(cs => MapScheduleToDto(cs))
            .ToListAsync();
    }

    private static InstructorDto MapToDto(Instructor i) => new()
    {
        Id = i.Id,
        FirstName = i.FirstName,
        LastName = i.LastName,
        Email = i.Email,
        Phone = i.Phone,
        Bio = i.Bio,
        Specializations = i.Specializations,
        HireDate = i.HireDate,
        IsActive = i.IsActive,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };

    private static ClassScheduleDto MapScheduleToDto(ClassSchedule cs) => new()
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
