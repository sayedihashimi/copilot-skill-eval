using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(FineStatus? status, int page, int pageSize)
    {
        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<FineResponse?> GetFineByIdAsync(int id)
    {
        return await db.Fines.AsNoTracking()
            .Include(f => f.Patron)
            .Where(f => f.Id == id)
            .Select(f => new FineResponse(f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<FineResponse> PayFineAsync(int id)
    {
        var fine = await db.Fines.Include(f => f.Patron).FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
        {
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");
        }

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} of ${Amount:F2} paid by patron {PatronId}", id, fine.Amount, fine.PatronId);

        return new FineResponse(fine.Id, fine.PatronId, $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }

    public async Task<FineResponse> WaiveFineAsync(int id)
    {
        var fine = await db.Fines.Include(f => f.Patron).FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
        {
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");
        }

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} of ${Amount:F2} waived for patron {PatronId}", id, fine.Amount, fine.PatronId);

        return new FineResponse(fine.Id, fine.PatronId, $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }
}
