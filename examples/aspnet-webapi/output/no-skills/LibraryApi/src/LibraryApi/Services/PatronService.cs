using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService : IPatronService
{
    private readonly LibraryDbContext _db;

    public PatronService(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<PatronDto>> GetAllAsync(string? search, string? membershipType, int page, int pageSize)
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
        var items = await query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<PatronDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PatronDetailDto?> GetByIdAsync(int id)
    {
        var patron = await _db.Patrons.FirstOrDefaultAsync(p => p.Id == id);
        if (patron == null) return null;

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0;

        return new PatronDetailDto
        {
            Id = patron.Id, FirstName = patron.FirstName, LastName = patron.LastName,
            Email = patron.Email, Phone = patron.Phone, Address = patron.Address,
            MembershipDate = patron.MembershipDate, MembershipType = patron.MembershipType.ToString(),
            IsActive = patron.IsActive, CreatedAt = patron.CreatedAt, UpdatedAt = patron.UpdatedAt,
            ActiveLoansCount = activeLoans, TotalUnpaidFines = unpaidFines
        };
    }

    public async Task<PatronDto> CreateAsync(PatronCreateDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new BusinessRuleException($"A patron with email '{dto.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Address = dto.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MembershipType = dto.MembershipType,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();
        return MapToDto(patron);
    }

    public async Task<PatronDto?> UpdateAsync(int id, PatronUpdateDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id);
        if (patron == null) return null;

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new BusinessRuleException($"A patron with email '{dto.Email}' already exists.");

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(patron);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id);
        if (patron == null) throw new NotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoans) throw new BusinessRuleException("Cannot deactivate a patron who has active or overdue loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        query = query.OrderByDescending(l => l.LoanDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId).OrderByDescending(r => r.ReservationDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(ReservationService.MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Fines.Include(f => f.Loan).ThenInclude(l => l.Book).Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        query = query.OrderByDescending(f => f.IssuedDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items.Select(FineService.MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    private static PatronDto MapToDto(Patron p) => new()
    {
        Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
        Email = p.Email, Phone = p.Phone, Address = p.Address,
        MembershipDate = p.MembershipDate, MembershipType = p.MembershipType.ToString(),
        IsActive = p.IsActive, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}
