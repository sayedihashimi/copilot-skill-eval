using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService : IFineService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<FineService> _logger;

    public FineService(LibraryDbContext db, ILogger<FineService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<FineDto>> GetFinesAsync(string? status, int page, int pageSize)
    {
        var query = _db.Fines.Include(f => f.Patron).Include(f => f.Loan).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items.Select(LoanService.MapFineToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<FineDto?> GetFineByIdAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id);
        return fine == null ? null : LoanService.MapFineToDto(fine);
    }

    public async Task<(FineDto? Fine, string? Error)> PayFineAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fine == null) return (null, "Fine not found.");
        if (fine.Status == FineStatus.Paid) return (null, "Fine has already been paid.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine paid: {FineId}, Amount ${Amount}", fine.Id, fine.Amount);
        return (LoanService.MapFineToDto(fine), null);
    }

    public async Task<(FineDto? Fine, string? Error)> WaiveFineAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fine == null) return (null, "Fine not found.");
        if (fine.Status == FineStatus.Paid) return (null, "Fine has already been paid and cannot be waived.");
        if (fine.Status == FineStatus.Waived) return (null, "Fine has already been waived.");

        fine.Status = FineStatus.Waived;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine waived: {FineId}, Amount ${Amount}", fine.Id, fine.Amount);
        return (LoanService.MapFineToDto(fine), null);
    }
}
