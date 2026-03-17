using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db) : IFineService
{
    public async Task<PaginatedResponse<FineDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineDto(f.Id, f.PatronId,
                $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync(ct);

        return new PaginatedResponse<FineDto>(items, total, page, pageSize);
    }

    public async Task<FineDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Fines.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FineDto(f.Id, f.PatronId,
                $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(FineDto? Fine, string? Error)> PayAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines.FindAsync([id], ct);
        if (fine is null) return (null, "Fine not found.");

        if (fine.Status == FineStatus.Paid) return (null, "Fine has already been paid.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct), null);
    }

    public async Task<(FineDto? Fine, string? Error)> WaiveAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines.FindAsync([id], ct);
        if (fine is null) return (null, "Fine not found.");

        if (fine.Status == FineStatus.Paid) return (null, "Fine has already been paid and cannot be waived.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine has already been waived.");

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct), null);
    }
}
