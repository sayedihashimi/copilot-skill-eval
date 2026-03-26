using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext context, ILogger<FineService> logger) : IFineService
{
    public async Task<PagedResult<FineResponse>> GetFinesAsync(string? status, int page, int pageSize)
    {
        var query = context.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => MapFineResponse(f))
            .ToListAsync();

        return new PagedResult<FineResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FineResponse?> GetFineByIdAsync(int id)
    {
        return await context.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.Id == id)
            .Select(f => MapFineResponse(f))
            .FirstOrDefaultAsync();
    }

    public async Task<FineResponse> PayFineAsync(int id)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} of ${Amount:F2} paid by patron {PatronId}", id, fine.Amount, fine.PatronId);

        return MapFineResponse(fine);
    }

    public async Task<FineResponse> WaiveFineAsync(int id)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");

        fine.Status = FineStatus.Waived;
        await context.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} of ${Amount:F2} waived for patron {PatronId}", id, fine.Amount, fine.PatronId);

        return MapFineResponse(fine);
    }

    internal static FineResponse MapFineResponse(Fine f) => new()
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
        Status = f.Status.ToString(),
        CreatedAt = f.CreatedAt
    };
}
