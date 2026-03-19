using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService : IPatronService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<PatronService> _logger;

    public PatronService(LibraryDbContext db, ILogger<PatronService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize)
    {
        var query = _db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(s) || p.LastName.ToLower().Contains(s) || p.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
            query = query.Where(p => p.MembershipType == mt);

        var total = await query.CountAsync();
        var patrons = await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = new List<PatronDto>();
        foreach (var p in patrons)
            items.Add(await MapToDto(p));

        return new PagedResult<PatronDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PatronDto?> GetPatronByIdAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id);
        return patron == null ? null : await MapToDto(patron);
    }

    public async Task<PatronDto> CreatePatronAsync(CreatePatronDto dto)
    {
        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipType = dto.MembershipType,
            MembershipDate = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Patron created: {Id} {Name}", patron.Id, $"{patron.FirstName} {patron.LastName}");
        return (await GetPatronByIdAsync(patron.Id))!;
    }

    public async Task<PatronDto?> UpdatePatronAsync(int id, UpdatePatronDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id);
        if (patron == null) return null;

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (await GetPatronByIdAsync(id))!;
    }

    public async Task<(bool Success, string? Error)> DeactivatePatronAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id);
        if (patron == null) return (false, "Patron not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans) return (false, "Cannot deactivate patron with active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(BookService.MapLoanToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId).OrderByDescending(r => r.ReservationDate);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(BookService.MapReservationToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        var query = _db.Fines.Include(f => f.Patron).Include(f => f.Loan)
            .Where(f => f.PatronId == patronId);

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

    private async Task<PatronDto> MapToDto(Patron p)
    {
        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == p.Id && l.Status == LoanStatus.Active);
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == p.Id && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0m;

        return new PatronDto
        {
            Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
            Email = p.Email, Phone = p.Phone, Address = p.Address,
            MembershipDate = p.MembershipDate, MembershipType = p.MembershipType,
            IsActive = p.IsActive, ActiveLoansCount = activeLoans,
            TotalUnpaidFines = unpaidFines, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
        };
    }
}
