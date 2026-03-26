using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db, ILogger<FineService> logger)
    : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetAllAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<FineStatus>(status, true, out var fineStatus))
            query = query.Where(f => f.Status == fineStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(FineServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<FineResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        return fine is null ? null : FineServiceHelper.MapToResponse(fine);
    }

    public async Task<FineResponse> PayAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new ArgumentException($"Fine is already '{fine.Status}' and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} of ${Amount:F2} paid by patron {PatronId}",
            fine.Id, fine.Amount, fine.PatronId);

        return FineServiceHelper.MapToResponse(fine);
    }

    public async Task<FineResponse> WaiveAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new ArgumentException($"Fine is already '{fine.Status}' and cannot be waived.");

        fine.Status = FineStatus.Waived;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} of ${Amount:F2} waived for patron {PatronId}",
            fine.Id, fine.Amount, fine.PatronId);

        return FineServiceHelper.MapToResponse(fine);
    }
}

internal static class FineServiceHelper
{
    public static FineResponse MapToResponse(Fine f) =>
        new(f.Id, f.PatronId,
            $"{f.Patron.FirstName} {f.Patron.LastName}",
            f.LoanId, f.Amount, f.Reason,
            f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt);
}
