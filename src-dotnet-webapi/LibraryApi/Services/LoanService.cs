using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService(LibraryDbContext db) : ILoanService
{
    private static int GetBorrowingLimit(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    private static int GetLoanDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    public async Task<PagedResponse<LoanResponse>> GetAllAsync(
        LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (overdue == true)
            query = query.Where(l => l.Status != LoanStatus.Returned && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        query = query.OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Paginate(items.Select(MapToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        return loan is null ? null : MapToResponse(loan);
    }

    public async Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron membership is not active.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies of this book are currently available.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to check out books.");

        // Check borrowing limit
        var activeLoans = await db.Loans
            .CountAsync(l => l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        var limit = GetBorrowingLimit(patron.MembershipType);
        if (activeLoans >= limit)
            throw new InvalidOperationException($"Patron has reached the borrowing limit of {limit} for {patron.MembershipType} membership.");

        // Check if patron already has this book on active loan
        var alreadyBorrowed = await db.Loans
            .AnyAsync(l => l.BookId == book.Id && l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (alreadyBorrowed)
            throw new InvalidOperationException("Patron already has an active loan for this book.");

        var now = DateTime.UtcNow;
        var loanDays = GetLoanDays(patron.MembershipType);

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await db.Entry(loan).Reference(l => l.Book).LoadAsync(ct);
        await db.Entry(loan).Reference(l => l.Patron).LoadAsync(ct);
        return MapToResponse(loan);
    }

    public async Task<ReturnLoanResponse> ReturnAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        decimal? fineAmount = null;
        string? fineReason = null;

        // Auto-generate fine for overdue
        if (loan.DueDate < now)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            fineAmount = daysOverdue * 0.25m;
            fineReason = $"Overdue return - {daysOverdue} day(s) late at $0.25/day";

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount.Value,
                Reason = fineReason,
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            db.Fines.Add(fine);
        }

        // Promote first pending reservation to Ready
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);

            // Reserve the copy
            loan.Book.AvailableCopies--;
        }

        await db.SaveChangesAsync(ct);

        return new ReturnLoanResponse
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BookISBN = loan.Book.ISBN,
            PatronId = loan.PatronId,
            PatronName = loan.Patron.FirstName + " " + loan.Patron.LastName,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            RenewalCount = loan.RenewalCount,
            CreatedAt = loan.CreatedAt,
            FineAmount = fineAmount,
            FineReason = fineReason
        };
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue)
            throw new InvalidOperationException("Cannot renew an overdue loan. Please return the book first.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum renewal limit (2) has been reached.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to renew.");

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew because there are pending reservations for this book.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);
        return MapToResponse(loan);
    }

    public async Task<List<LoanResponse>> GetOverdueAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var loans = await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.ReturnDate == null && l.DueDate < now)
            .OrderBy(l => l.DueDate)
            .ToListAsync(ct);

        // Update status for detected overdue loans
        var loanIds = loans.Select(l => l.Id).ToList();
        await db.Loans
            .Where(l => loanIds.Contains(l.Id) && l.Status == LoanStatus.Active)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.Status, LoanStatus.Overdue), ct);

        return loans.Select(l =>
        {
            var resp = MapToResponse(l);
            resp.Status = LoanStatus.Overdue;
            return resp;
        }).ToList();
    }

    private static LoanResponse MapToResponse(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        BookISBN = l.Book.ISBN,
        PatronId = l.PatronId,
        PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        Status = l.Status,
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
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
