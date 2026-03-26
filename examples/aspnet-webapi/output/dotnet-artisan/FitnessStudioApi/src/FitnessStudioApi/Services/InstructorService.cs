using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(FitnessDbContext db) : IInstructorService
{
    public async Task<List<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, CancellationToken ct = default)
    {
        var query = db.Instructors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var term = specialization.ToLower();
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(i => i.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");
        }

        var instructor = new Instructor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Bio = request.Bio,
            Specializations = request.Specializations,
            HireDate = request.HireDate,
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync(ct);

        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct = default)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null)
        {
            return null;
        }

        if (await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct))
        {
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");
        }

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(instructor);
    }

    public async Task<List<ClassScheduleListResponse>> GetScheduleAsync(
        int instructorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId, ct))
        {
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");
        }

        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
        {
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(cs => cs.StartTime <= toDate.Value);
        }

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleListResponse(
                cs.Id, cs.ClassType.Name,
                cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
                cs.Capacity - cs.CurrentEnrollment, cs.Room, cs.Status))
            .ToListAsync(ct);
    }

    private static InstructorResponse MapToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone, i.Bio,
        i.Specializations, i.HireDate, i.IsActive, i.CreatedAt, i.UpdatedAt);
}
