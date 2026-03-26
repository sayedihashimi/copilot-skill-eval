using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class InstructorService : IInstructorService
{
    private readonly FitnessDbContext _db;

    public InstructorService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResult<InstructorDto>> GetAllAsync(int page, int pageSize, string? specialization, bool? isActive)
    {
        var query = _db.Instructors.AsQueryable();
        if (isActive.HasValue) query = query.Where(i => i.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        var total = await query.CountAsync();
        var items = await query.OrderBy(i => i.LastName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(i => ToDto(i)).ToListAsync();

        return new PaginatedResult<InstructorDto>(items, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<InstructorDto?> GetByIdAsync(int id)
    {
        var i = await _db.Instructors.FindAsync(id);
        return i == null ? null : ToDto(i);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Conflict");

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

        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync();
        return ToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _db.Instructors.FindAsync(id)
            ?? throw new BusinessRuleException("Instructor not found.", 404, "Not Found");

        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Conflict");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(instructor);
    }

    public async Task<IEnumerable<ClassScheduleListDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new BusinessRuleException("Instructor not found.", 404, "Not Found");

        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync();
        return schedules.Select(cs => new ClassScheduleListDto(
            cs.Id, cs.ClassType.Name, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment), cs.Room, cs.Status.ToString()));
    }

    private static InstructorDto ToDto(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);
}
