using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(
        string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct)
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

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PatronResponse(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<PatronResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PatronDetailResponse?> GetPatronByIdAsync(int id, CancellationToken ct)
    {
        return await db.Patrons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive,
                p.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount),
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request, CancellationToken ct)
    {
        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MembershipType = request.MembershipType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Patron created: {Id} - {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone,
            patron.Address, patron.MembershipDate, patron.MembershipType, patron.IsActive, patron.CreatedAt, patron.UpdatedAt);
    }

    public async Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request, CancellationToken ct)
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
        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone,
            patron.Address, patron.MembershipDate, patron.MembershipType, patron.IsActive, patron.CreatedAt, patron.UpdatedAt);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeactivatePatronAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.Include(p => p.Loans).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (patron is null) return (false, false);
        if (patron.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            return (true, true);

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Patron deactivated: {Id}", id);
        return (true, false);
    }

    public async Task<IReadOnlyList<LoanResponse>?> GetPatronLoansAsync(int patronId, string? status, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct)) return null;

        var query = db.Loans.AsNoTracking()
            .Where(l => l.PatronId == patronId)
            .Include(l => l.Book).Include(l => l.Patron);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus).Include(l => l.Book).Include(l => l.Patron);

        return await query
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReservationResponse>?> GetPatronReservationsAsync(int patronId, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct)) return null;

        return await db.Reservations.AsNoTracking()
            .Where(r => r.PatronId == patronId)
            .Include(r => r.Book).Include(r => r.Patron)
            .OrderByDescending(r => r.ReservationDate)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FineResponse>?> GetPatronFinesAsync(int patronId, string? status, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct)) return null;

        var query = db.Fines.AsNoTracking()
            .Where(f => f.PatronId == patronId)
            .Include(f => f.Patron).Include(f => f.Loan);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fineStatus))
            query = query.Where(f => f.Status == fineStatus).Include(f => f.Patron).Include(f => f.Loan);

        return await query
            .OrderByDescending(f => f.IssuedDate)
            .Select(f => new FineResponse(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync(ct);
    }
}
