using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize)
    {
        var query = db.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
        {
            query = query.Where(p => p.MembershipType == mt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType.ToString(), p.IsActive, p.CreatedAt, p.UpdatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<PatronResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PatronResponse?> GetPatronByIdAsync(int id)
    {
        return await db.Patrons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatronResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType.ToString(), p.IsActive, p.CreatedAt, p.UpdatedAt,
                p.Loans.Count(l => l.Status == LoanStatus.Active),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request)
    {
        if (await db.Patrons.AnyAsync(p => p.Email == request.Email))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipType = Enum.Parse<MembershipType>(request.MembershipType, ignoreCase: true)
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync();

        logger.LogInformation("Patron created: {PatronId} {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return new PatronResponse(
            patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone, patron.Address,
            patron.MembershipDate, patron.MembershipType.ToString(), patron.IsActive, patron.CreatedAt, patron.UpdatedAt,
            0, 0m
        );
    }

    public async Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) return null;

        if (await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = Enum.Parse<MembershipType>(request.MembershipType, ignoreCase: true);
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Patron updated: {PatronId}", id);

        return (await GetPatronByIdAsync(id))!;
    }

    public async Task<bool> DeactivatePatronAsync(int id)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null)
            throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new InvalidOperationException("Cannot deactivate a patron who has active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Patron deactivated: {PatronId}", id);
        return true;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans.AsNoTracking()
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status.ToString(), l.RenewalCount, l.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Reservations.AsNoTracking()
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status.ToString(), r.QueuePosition, r.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines.AsNoTracking()
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(
                f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason, f.IssuedDate, f.PaidDate,
                f.Status.ToString(), f.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}
