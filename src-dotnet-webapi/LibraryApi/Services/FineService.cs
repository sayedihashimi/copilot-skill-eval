using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService(LibraryDbContext db) : IFineService
{
    public async Task<PagedResponse<FineResponse>> GetAllAsync(FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        query = query.OrderByDescending(f => f.IssuedDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Paginate(items.Select(MapToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines.AsNoTracking()
            .Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        return fine is null ? null : MapToResponse(fine);
    }

    public async Task<FineResponse> PayAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already {fine.Status}.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return MapToResponse(fine);
    }

    public async Task<FineResponse> WaiveAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already {fine.Status}.");

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync(ct);

        return MapToResponse(fine);
    }

    private static FineResponse MapToResponse(Fine f) => new()
    {
        Id = f.Id,
        PatronId = f.PatronId,
        PatronName = f.Patron.FirstName + " " + f.Patron.LastName,
        LoanId = f.LoanId,
        BookTitle = f.Loan.Book.Title,
        Amount = f.Amount,
        Reason = f.Reason,
        IssuedDate = f.IssuedDate,
        PaidDate = f.PaidDate,
        Status = f.Status,
        CreatedAt = f.CreatedAt
    };

    private static PagedResponse<T> Paginate<T>(List<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResponse<T>
        {
            Items = items, Page = page, PageSize = pageSize,
            TotalCount = totalCount, TotalPages = totalPages,
            HasNextPage = page < totalPages, HasPreviousPage = page > 1
        };
    }
}
