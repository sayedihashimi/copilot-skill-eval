using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorResponse> GetByIdAsync(int id);
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request);
    Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request);
    Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to);
}

public class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = db.Instructors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToResponse(i))
            .ToListAsync();
    }

    public async Task<InstructorResponse> GetByIdAsync(int id)
    {
        var instructor = await db.Instructors.FindAsync(id)
            ?? throw new NotFoundException($"Instructor with ID {id} not found");
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == request.Email))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists");

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
        await db.SaveChangesAsync();
        logger.LogInformation("Created instructor: {Email}", instructor.Email);
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request)
    {
        var instructor = await db.Instructors.FindAsync(id)
            ?? throw new NotFoundException($"Instructor with ID {id} not found");

        if (await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists");

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.HireDate = request.HireDate;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated instructor: {InstructorId}", id);
        return MapToResponse(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new NotFoundException($"Instructor with ID {instructorId} not found");

        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync();
        return schedules.Select(ClassScheduleService.MapToResponse).ToList();
    }

    private static InstructorResponse MapToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone, i.Bio,
        i.Specializations, i.HireDate, i.IsActive, i.CreatedAt, i.UpdatedAt);
}
