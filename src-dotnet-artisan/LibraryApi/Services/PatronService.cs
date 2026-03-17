using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db) : IPatronService
{
    public async Task<PaginatedResponse<PatronDto>> GetAllAsync(string? search, string? membershipType, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
        {
            query = query.Where(p => p.MembershipType == mt);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronDto(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.MembershipType, p.IsActive, p.MembershipDate))
            .ToListAsync(ct);

        return new PaginatedResponse<PatronDto>(items, total, page, pageSize);
    }

    public async Task<PatronDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Patrons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailDto(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipType, p.IsActive, p.MembershipDate, p.CreatedAt,
                p.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatronDto> CreateAsync(CreatePatronRequest request, CancellationToken ct)
    {
        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipType = request.MembershipType
        };
        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);

        return new PatronDto(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone, patron.MembershipType, patron.IsActive, patron.MembershipDate);
    }

    public async Task<PatronDto?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return null;

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return new PatronDto(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone, patron.MembershipType, patron.IsActive, patron.MembershipDate);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeactivateAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return (false, false);

        var hasActive = await db.Loans.AnyAsync(l => l.PatronId == id && l.Status != LoanStatus.Returned, ct);
        if (hasActive) return (true, true);

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, false);
    }

    public async Task<PaginatedResponse<LoanDto>> GetLoansAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking().Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(int patronId, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FineDto>> GetFinesAsync(int patronId, string? status, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking().Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        return await query
            .OrderByDescending(f => f.IssuedDate)
            .Select(f => new FineDto(f.Id, f.PatronId,
                $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync(ct);
    }
}
