using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService : IInstructorService
{
    private readonly FitnessDbContext _db;

    public InstructorService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResponse<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Instructors.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => ToResponse(i))
            .ToListAsync(ct);

        return new PaginatedResponse<InstructorResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await _db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return instructor is null ? null : ToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == request.Email, ct))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists.", 409);

        var instructor = new Instructor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Bio = request.Bio,
            Specializations = request.Specializations,
            HireDate = request.HireDate,
            IsActive = request.IsActive
        };

        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync(ct);
        return ToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await _db.Instructors.FindAsync([id], ct);
        if (instructor is null) return null;

        if (await _db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct))
            throw new BusinessRuleException($"An instructor with email '{request.Email}' already exists.", 409);

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.HireDate = request.HireDate;
        instructor.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToResponse(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId, ct))
            throw new KeyNotFoundException($"Instructor with id {instructorId} not found.");

        var query = _db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity,
                cs.CurrentEnrollment, cs.WaitlistCount,
                Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
                cs.Room, cs.Status.ToString(), cs.CancellationReason,
                cs.CreatedAt, cs.UpdatedAt))
            .ToListAsync(ct);
    }

    private static InstructorResponse ToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);
}
