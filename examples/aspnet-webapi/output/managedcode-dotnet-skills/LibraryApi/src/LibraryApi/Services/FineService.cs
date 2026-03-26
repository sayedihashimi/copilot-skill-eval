using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(string? status, int page, int pageSize)
    {
        var query = db.Fines.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(
                f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason, f.IssuedDate, f.PaidDate,
                f.Status.ToString(), f.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<FineResponse?> GetFineByIdAsync(int id)
    {
        return await db.Fines.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FineResponse(
                f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason, f.IssuedDate, f.PaidDate,
                f.Status.ToString(), f.CreatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<FineResponse> PayFineAsync(int id)
    {
        var fine = await db.Fines.FindAsync(id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status == FineStatus.Paid)
            throw new InvalidOperationException("This fine has already been paid.");

        if (fine.Status == FineStatus.Waived)
            throw new InvalidOperationException("This fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Fine paid: FineId={FineId}, Amount=${Amount}", id, fine.Amount);

        return (await GetFineByIdAsync(id))!;
    }

    public async Task<FineResponse> WaiveFineAsync(int id)
    {
        var fine = await db.Fines.FindAsync(id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status == FineStatus.Paid)
            throw new InvalidOperationException("This fine has already been paid and cannot be waived.");

        if (fine.Status == FineStatus.Waived)
            throw new InvalidOperationException("This fine has already been waived.");

        fine.Status = FineStatus.Waived;

        await db.SaveChangesAsync();

        logger.LogInformation("Fine waived: FineId={FineId}, Amount=${Amount}", id, fine.Amount);

        return (await GetFineByIdAsync(id))!;
    }
}
