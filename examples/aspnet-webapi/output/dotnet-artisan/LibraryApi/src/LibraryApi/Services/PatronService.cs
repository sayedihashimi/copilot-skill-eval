using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, MembershipType? membershipType, int page, int pageSize)
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
        {
            query = query.Where(p => p.MembershipType == membershipType.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronResponse(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive, p.CreatedAt, p.UpdatedAt))
            .ToListAsync();

        return new PaginatedResponse<PatronResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PatronResponse?> GetPatronByIdAsync(int id)
    {
        var patron = await db.Patrons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (patron is null) return null;

        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        var unpaidFines = await db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);

        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone,
            patron.Address, patron.MembershipDate, patron.MembershipType, patron.IsActive,
            patron.CreatedAt, patron.UpdatedAt, activeLoans, unpaidFines);
    }

    public async Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request)
    {
        if (await db.Patrons.AnyAsync(p => p.Email == request.Email))
        {
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");
        }

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

        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.Phone,
            patron.Address, patron.MembershipDate, patron.MembershipType, patron.IsActive,
            patron.CreatedAt, patron.UpdatedAt, 0, 0m);
    }

    public async Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null) return null;

        if (await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id))
        {
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");
        }

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetPatronByIdAsync(id);
    }

    public async Task<bool> DeactivatePatronAsync(int id)
    {
        var patron = await db.Patrons.FindAsync(id);
        if (patron is null)
        {
            throw new KeyNotFoundException($"Patron with ID {id} not found.");
        }

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
        {
            throw new InvalidOperationException($"Cannot deactivate patron '{patron.FirstName} {patron.LastName}' because they have active loans.");
        }

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Deactivated patron {PatronId}", id);
        return true;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
        {
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");
        }

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName, l.LoanDate, l.DueDate,
                l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
        {
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");
        }

        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName, r.ReservationDate, r.ExpirationDate,
                r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status, int page, int pageSize)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId))
        {
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");
        }

        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(f.Id, f.PatronId, f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}
