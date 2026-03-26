using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs.ClassSchedule;
using FitnessStudioApi.DTOs.Instructor;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService : IInstructorService
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
        var query = _context.Instructors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var pattern = $"%{specialization}%";
            query = query.Where(i => i.Specializations != null && EF.Functions.Like(i.Specializations, pattern));
        }

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        return await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<InstructorDto> GetByIdAsync(int id)
    {
        var instructor = await _context.Instructors.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");

        return MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _context.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Duplicate Resource");

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

        _logger.LogInformation("Created instructor: {InstructorName} (ID: {InstructorId})", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _context.Instructors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");

        if (await _context.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Duplicate Resource");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated instructor: {InstructorName} (ID: {InstructorId})", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _context.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = _context.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleDto
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
            })
            .ToListAsync();
    }

    private static InstructorDto MapToDto(Instructor instructor) => new()
    {
        Id = instructor.Id,
        FirstName = instructor.FirstName,
        LastName = instructor.LastName,
        Email = instructor.Email,
        Phone = instructor.Phone,
        Bio = instructor.Bio,
        Specializations = instructor.Specializations,
        HireDate = instructor.HireDate,
        IsActive = instructor.IsActive,
        CreatedAt = instructor.CreatedAt,
        UpdatedAt = instructor.UpdatedAt
    };
}
