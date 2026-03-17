using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VeterinarianService(VetClinicDbContext context) : IVeterinarianService
{
    private readonly VetClinicDbContext _context = context;

    public async Task<PaginatedResponse<VeterinarianDto>> GetAllAsync(
        string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Veterinarians.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToDto(v))
            .ToListAsync(ct);

        return new PaginatedResponse<VeterinarianDto>(items, page, pageSize, totalCount);
    }

    public async Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Veterinarians
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => MapToDto(v))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<VeterinarianDto> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct)
    {
        var emailTaken = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.Email == request.Email, ct);
        if (emailTaken)
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        var licenseTaken = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.LicenseNumber == request.LicenseNumber, ct);
        if (licenseTaken)
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        var vet = new Veterinarian
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            HireDate = request.HireDate
        };

        _context.Veterinarians.Add(vet);
        await _context.SaveChangesAsync(ct);

        return MapToDto(vet);
    }

    public async Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await _context.Veterinarians.FindAsync([id], ct);
        if (vet is null) return null;

        var emailTaken = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.Email == request.Email && v.Id != id, ct);
        if (emailTaken)
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        var licenseTaken = await _context.Veterinarians.AsNoTracking()
            .AnyAsync(v => v.LicenseNumber == request.LicenseNumber && v.Id != id, ct);
        if (licenseTaken)
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        vet.FirstName = request.FirstName;
        vet.LastName = request.LastName;
        vet.Email = request.Email;
        vet.Phone = request.Phone;
        vet.Specialization = request.Specialization;
        vet.LicenseNumber = request.LicenseNumber;
        vet.IsAvailable = request.IsAvailable;
        vet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return MapToDto(vet);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct)
    {
        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId &&
                        a.AppointmentDate >= dateStart &&
                        a.AppointmentDate <= dateEnd &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<AppointmentDto>> GetAppointmentsAsync(
        int vetId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentDto>(items, page, pageSize, totalCount);
    }

    private static VeterinarianDto MapToDto(Veterinarian v) =>
        new(v.Id, v.FirstName, v.LastName, v.Email, v.Phone,
            v.Specialization, v.LicenseNumber, v.IsAvailable,
            v.HireDate, v.CreatedAt, v.UpdatedAt);
}
