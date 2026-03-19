using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VeterinarianService(VetClinicDbContext context, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PagedResult<VeterinarianResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = context.Veterinarians.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync(ct);

        return new PagedResult<VeterinarianResponse>(items, totalCount, page, pageSize);
    }

    public async Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Veterinarians
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => MapToResponse(v))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct)
    {
        if (await context.Veterinarians.AnyAsync(v => v.Email == request.Email, ct))
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        if (await context.Veterinarians.AnyAsync(v => v.LicenseNumber == request.LicenseNumber, ct))
            throw new InvalidOperationException($"A veterinarian with license '{request.LicenseNumber}' already exists.");

        var vet = new Veterinarian
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            HireDate = request.HireDate,
        };

        context.Veterinarians.Add(vet);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created veterinarian {VetId} ({LicenseNumber})", vet.Id, vet.LicenseNumber);
        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await context.Veterinarians.FindAsync([id], ct);
        if (vet is null) return null;

        if (await context.Veterinarians.AnyAsync(v => v.Email == request.Email && v.Id != id, ct))
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        if (await context.Veterinarians.AnyAsync(v => v.LicenseNumber == request.LicenseNumber && v.Id != id, ct))
            throw new InvalidOperationException($"A veterinarian with license '{request.LicenseNumber}' already exists.");

        vet.FirstName = request.FirstName;
        vet.LastName = request.LastName;
        vet.Email = request.Email;
        vet.Phone = request.Phone;
        vet.Specialization = request.Specialization;
        vet.LicenseNumber = request.LicenseNumber;
        vet.IsAvailable = request.IsAvailable;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Updated veterinarian {VetId}", vet.Id);
        return MapToResponse(vet);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId
                && a.AppointmentDate >= startOfDay
                && a.AppointmentDate <= endOfDay
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int vetId, CancellationToken ct)
    {
        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    private static VeterinarianResponse MapToResponse(Veterinarian v) => new(
        v.Id, v.FirstName, v.LastName, v.Email, v.Phone,
        v.Specialization, v.LicenseNumber, v.IsAvailable, v.HireDate);
}
