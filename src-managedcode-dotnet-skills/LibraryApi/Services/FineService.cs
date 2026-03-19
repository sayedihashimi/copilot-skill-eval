using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService(LibraryDbContext context, ILogger<FineService> logger) : IFineService
{
    public async Task<PagedResult<FineDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Fines.CountAsync();
        var items = await context.Fines
            .AsNoTracking()
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineDto(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync();

        return new PagedResult<FineDto>(items, totalCount, page, pageSize);
    }

    public async Task<FineDto?> GetByIdAsync(int id)
    {
        return await context.Fines
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FineDto(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<FineDto> PayAsync(int id)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} paid by Patron {PatronId}, Amount: ${Amount:F2}", id, fine.PatronId, fine.Amount);

        return new FineDto(fine.Id, fine.PatronId,
            $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }

    public async Task<FineDto> WaiveAsync(int id)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");

        fine.Status = FineStatus.Waived;

        await context.SaveChangesAsync();

        logger.LogInformation("Fine {FineId} waived for Patron {PatronId}, Amount: ${Amount:F2}", id, fine.PatronId, fine.Amount);

        return new FineDto(fine.Id, fine.PatronId,
            $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Amount, fine.Reason, fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }
}
