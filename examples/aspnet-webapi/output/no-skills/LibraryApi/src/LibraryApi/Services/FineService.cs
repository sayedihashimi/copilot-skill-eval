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

    public async Task<PagedResult<FineDto>> GetAllAsync(string? status, int page, int pageSize)
    {
        var query = _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        query = query.OrderByDescending(f => f.IssuedDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<FineDto?> GetByIdAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).FirstOrDefaultAsync(f => f.Id == id);
        return fine == null ? null : MapToDto(fine);
    }

    public async Task<FineDto> PayAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status == FineStatus.Paid)
            throw new BusinessRuleException("This fine has already been paid.");
        if (fine.Status == FineStatus.Waived)
            throw new BusinessRuleException("This fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} of ${Amount:F2} paid by patron {PatronId}", id, fine.Amount, fine.PatronId);
        return MapToDto(fine);
    }

    public async Task<FineDto> WaiveAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status == FineStatus.Paid)
            throw new BusinessRuleException("This fine has already been paid and cannot be waived.");
        if (fine.Status == FineStatus.Waived)
            throw new BusinessRuleException("This fine has already been waived.");

        fine.Status = FineStatus.Waived;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} of ${Amount:F2} waived for patron {PatronId}", id, fine.Amount, fine.PatronId);
        return MapToDto(fine);
    }

    internal static FineDto MapToDto(Fine f) => new()
    {
        Id = f.Id, PatronId = f.PatronId, PatronName = $"{f.Patron.FirstName} {f.Patron.LastName}",
        LoanId = f.LoanId, BookTitle = f.Loan.Book.Title,
        Amount = f.Amount, Reason = f.Reason,
        IssuedDate = f.IssuedDate, PaidDate = f.PaidDate,
        Status = f.Status.ToString(), CreatedAt = f.CreatedAt
    };
}
