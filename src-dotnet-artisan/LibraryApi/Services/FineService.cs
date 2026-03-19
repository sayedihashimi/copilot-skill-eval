using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PagedResult<FineResponse>> GetAllAsync(FineStatus? status, int page, int pageSize);
    Task<FineResponse?> GetByIdAsync(int id);
    Task<(FineResponse? Fine, string? Error)> PayAsync(int id);
    Task<(FineResponse? Fine, string? Error)> WaiveAsync(int id);
}

public class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PagedResult<FineResponse>> GetAllAsync(FineStatus? status, int page, int pageSize)
    {
        var query = db.Fines
            .Include(f => f.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(
                f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync();

        return new PagedResult<FineResponse>(items, totalCount, page, pageSize);
    }

    public async Task<FineResponse?> GetByIdAsync(int id)
    {
        return await db.Fines
            .Where(f => f.Id == id)
            .Include(f => f.Patron)
            .Select(f => new FineResponse(
                f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .FirstOrDefaultAsync();
    }

    public async Task<(FineResponse? Fine, string? Error)> PayAsync(int id)
    {
        var fine = await db.Fines.Include(f => f.Patron).FirstOrDefaultAsync(f => f.Id == id);
        if (fine is null) return (null, "Fine not found.");
        if (fine.Status == FineStatus.Paid) return (null, "Fine is already paid.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine has been waived.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} paid by patron {PatronId}", fine.Id, fine.PatronId);

        return (new FineResponse(
            fine.Id, fine.PatronId,
            fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status), null);
    }

    public async Task<(FineResponse? Fine, string? Error)> WaiveAsync(int id)
    {
        var fine = await db.Fines.Include(f => f.Patron).FirstOrDefaultAsync(f => f.Id == id);
        if (fine is null) return (null, "Fine not found.");
        if (fine.Status == FineStatus.Paid) return (null, "Fine is already paid.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine is already waived.");

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} waived for patron {PatronId}", fine.Id, fine.PatronId);

        return (new FineResponse(
            fine.Id, fine.PatronId,
            fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status), null);
    }
}
