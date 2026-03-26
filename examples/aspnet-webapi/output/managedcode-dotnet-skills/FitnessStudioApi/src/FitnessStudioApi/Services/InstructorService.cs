using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorDto>> GetAllAsync(
        string? specialization, bool? isActive, CancellationToken ct)
    {
        var query = db.Instructors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null &&
                i.Specializations.ToLower().Contains(specialization.ToLower()));

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        return await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);
    }

    public async Task<InstructorDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await db.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return instructor is null ? null : MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto, CancellationToken ct)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == dto.Email, ct))
            throw new ConflictException($"An instructor with email '{dto.Email}' already exists.");

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

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created instructor {InstructorName} with Id {InstructorId}",
            $"{instructor.FirstName} {instructor.LastName}", instructor.Id);

        return MapToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct)
            ?? throw new NotFoundException($"Instructor with Id {id} not found.");

        if (await db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id, ct))
            throw new ConflictException($"An instructor with email '{dto.Email}' already exists.");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated instructor {InstructorId}", id);

        return MapToDto(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleListDto>> GetScheduleAsync(
        int instructorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId, ct))
            throw new NotFoundException($"Instructor with Id {instructorId} not found.");

        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleListDto(
                cs.Id,
                cs.ClassType.Name,
                $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
                cs.StartTime, cs.EndTime,
                cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment,
                cs.Room, cs.Status))
            .ToListAsync(ct);
    }

    private static InstructorDto MapToDto(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive);
}
