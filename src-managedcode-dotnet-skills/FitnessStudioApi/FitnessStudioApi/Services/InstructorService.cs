using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorResponse>> GetAllAsync(CancellationToken ct)
    {
        return await db.Instructors
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.LastName)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == request.Email, ct))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists.");

        var instructor = new Instructor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Bio = request.Bio,
            Specializations = request.Specializations,
            HireDate = request.HireDate
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created instructor {InstructorId}: {Name}",
            instructor.Id, $"{instructor.FirstName} {instructor.LastName}");
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null) return null;

        if (await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists.");

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated instructor {InstructorId}", instructor.Id);
        return MapToResponse(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, CancellationToken ct)
    {
        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId, ct))
            throw new BusinessRuleException("Instructor not found.");

        var now = DateTime.UtcNow;
        return await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId && cs.StartTime >= now)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
                cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
                cs.WaitlistCount, cs.Room, cs.Status, cs.CancellationReason,
                cs.ClassType.IsPremium, cs.Capacity - cs.CurrentEnrollment,
                cs.CreatedAt, cs.UpdatedAt))
            .ToListAsync(ct);
    }

    private static InstructorResponse MapToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);
}
