using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PagedResult<PatronResponse>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize);
    Task<PatronDetailResponse?> GetByIdAsync(int id);
    Task<PatronResponse> CreateAsync(CreatePatronRequest request);
    Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<List<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status);
    Task<List<ReservationResponse>> GetPatronReservationsAsync(int patronId);
    Task<List<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status);
}

public class PatronService(LibraryDbContext db) : IPatronService
{
    public async Task<PagedResult<PatronResponse>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize)
    {
        var query = db.Patrons.AsQueryable();

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

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronResponse(p.Id, p.FirstName, p.LastName, p.Email, p.MembershipType, p.MembershipDate, p.IsActive))
            .ToListAsync();

        return new PagedResult<PatronResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id)
    {
        return await db.Patrons
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipType, p.MembershipDate, p.IsActive,
                p.Loans.Count(l => l.Status == LoanStatus.Active),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount),
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<PatronResponse> CreateAsync(CreatePatronRequest request)
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
        await db.SaveChangesAsync();

        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.MembershipType, patron.MembershipDate, patron.IsActive);
    }

    public async Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) return null;

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.MembershipType, patron.MembershipDate, patron.IsActive);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) return (false, "Patron not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans) return (false, "Cannot deactivate patron with active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status)
    {
        var query = db.Loans
            .Where(l => l.PatronId == patronId)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        return await query
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();
    }

    public async Task<List<ReservationResponse>> GetPatronReservationsAsync(int patronId)
    {
        return await db.Reservations
            .Where(r => r.PatronId == patronId)
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .OrderByDescending(r => r.ReservationDate)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();
    }

    public async Task<List<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status)
    {
        var query = db.Fines
            .Where(f => f.PatronId == patronId)
            .Include(f => f.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        return await query
            .OrderByDescending(f => f.IssuedDate)
            .Select(f => new FineResponse(
                f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync();
    }
}
