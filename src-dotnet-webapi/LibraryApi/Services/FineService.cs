using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron).Include(f => f.Loan).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fineStatus))
            query = query.Where(f => f.Status == fineStatus);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new FineResponse(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<FineResponse?> GetFineByIdAsync(int id, CancellationToken ct)
    {
        return await db.Fines.AsNoTracking()
            .Include(f => f.Patron).Include(f => f.Loan)
            .Where(f => f.Id == id)
            .Select(f => new FineResponse(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(FineResponse? Fine, string? Error, bool NotFound)> PayFineAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines.Include(f => f.Patron).Include(f => f.Loan).FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fine is null) return (null, null, true);

        if (fine.Status == FineStatus.Paid)
            return (null, "This fine has already been paid.", false);

        if (fine.Status == FineStatus.Waived)
            return (null, "This fine has been waived and cannot be paid.", false);

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Fine paid: {Id}, Amount=${Amount}", id, fine.Amount);

        return (new FineResponse(fine.Id, fine.PatronId,
            $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt), null, false);
    }

    public async Task<(FineResponse? Fine, string? Error, bool NotFound)> WaiveFineAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines.Include(f => f.Patron).Include(f => f.Loan).FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fine is null) return (null, null, true);

        if (fine.Status == FineStatus.Paid)
            return (null, "This fine has already been paid and cannot be waived.", false);

        if (fine.Status == FineStatus.Waived)
            return (null, "This fine has already been waived.", false);

        fine.Status = FineStatus.Waived;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Fine waived: {Id}, Amount=${Amount}", id, fine.Amount);

        return (new FineResponse(fine.Id, fine.PatronId,
            $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt), null, false);
    }
}
