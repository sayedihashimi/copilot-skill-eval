using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService(LibraryDbContext db) : IPatronService
{
    public async Task<PagedResponse<PatronResponse>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct)
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

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return Paginate(items, page, pageSize, totalCount);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (patron is null) return null;

        var activeLoansCount = await db.Loans.CountAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        var unpaidFines = await db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount, ct);

        return new PatronDetailResponse
        {
            Id = patron.Id,
            FirstName = patron.FirstName,
            LastName = patron.LastName,
            Email = patron.Email,
            Phone = patron.Phone,
            Address = patron.Address,
            MembershipDate = patron.MembershipDate,
            MembershipType = patron.MembershipType,
            IsActive = patron.IsActive,
            CreatedAt = patron.CreatedAt,
            UpdatedAt = patron.UpdatedAt,
            ActiveLoansCount = activeLoansCount,
            UnpaidFinesBalance = unpaidFines
        };
    }

    public async Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct)
    {
        if (await db.Patrons.AnyAsync(p => p.Email == request.Email, ct))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

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
        return MapToResponse(patron);
    }

    public async Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return null;

        if (await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id, ct))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(patron);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        if (hasActiveLoans)
            throw new InvalidOperationException("Cannot deactivate patron with active or overdue loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        query = query.OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Paginate(items.Select(MapLoanResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<List<ReservationResponse>> GetPatronReservationsAsync(int patronId, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.ReservationDate)
            .Select(r => new ReservationResponse
            {
                Id = r.Id, BookId = r.BookId, BookTitle = r.Book.Title,
                PatronId = r.PatronId, PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
                ReservationDate = r.ReservationDate, ExpirationDate = r.ExpirationDate,
                Status = r.Status, QueuePosition = r.QueuePosition, CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        return await query.OrderByDescending(f => f.IssuedDate)
            .Select(f => new FineResponse
            {
                Id = f.Id, PatronId = f.PatronId,
                PatronName = f.Patron.FirstName + " " + f.Patron.LastName,
                LoanId = f.LoanId, BookTitle = f.Loan.Book.Title,
                Amount = f.Amount, Reason = f.Reason, IssuedDate = f.IssuedDate,
                PaidDate = f.PaidDate, Status = f.Status, CreatedAt = f.CreatedAt
            })
            .ToListAsync(ct);
    }

    private static PatronResponse MapToResponse(Patron p) => new()
    {
        Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
        Email = p.Email, Phone = p.Phone, Address = p.Address,
        MembershipDate = p.MembershipDate, MembershipType = p.MembershipType,
        IsActive = p.IsActive, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };

    private static LoanResponse MapLoanResponse(Loan l) => new()
    {
        Id = l.Id, BookId = l.BookId, BookTitle = l.Book.Title, BookISBN = l.Book.ISBN,
        PatronId = l.PatronId, PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
        LoanDate = l.LoanDate, DueDate = l.DueDate, ReturnDate = l.ReturnDate,
        Status = l.Status, RenewalCount = l.RenewalCount, CreatedAt = l.CreatedAt
    };

    private static PagedResponse<T> Paginate<T>(List<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResponse<T>
        {
            Items = items, Page = page, PageSize = pageSize,
            TotalCount = totalCount, TotalPages = totalPages,
            HasNextPage = page < totalPages, HasPreviousPage = page > 1
        };
    }
}
