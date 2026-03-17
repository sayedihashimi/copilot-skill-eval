using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<InstructorService> _logger = logger;

    public async Task<IReadOnlyList<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, CancellationToken ct)
    {
        var query = _db.Instructors.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.Contains(specialization));

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);
    }

    public async Task<InstructorResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await _db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        var emailExists = await _db.Instructors.AnyAsync(i => i.Email == request.Email, ct);
        if (emailExists)
            throw new ConflictException($"An instructor with email '{request.Email}' already exists.");

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

        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created instructor {Name} with ID {Id}", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await _db.Instructors.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");

        var emailConflict = await _db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct);
        if (emailConflict)
            throw new ConflictException($"An instructor with email '{request.Email}' already exists.");

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        _ = await _db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == instructorId, ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = _db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
                cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
                Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
                cs.Room, cs.Status, cs.CancellationReason,
                cs.ClassType.IsPremium, cs.CreatedAt, cs.UpdatedAt))
            .ToListAsync(ct);
    }

    private static InstructorResponse MapToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);
}
