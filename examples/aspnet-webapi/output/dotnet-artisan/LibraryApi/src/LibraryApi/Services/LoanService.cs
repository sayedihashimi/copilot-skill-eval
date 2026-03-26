using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private static readonly Dictionary<MembershipType, (int MaxLoans, int LoanDays)> BorrowingLimits = new()
    {
        [MembershipType.Standard] = (5, 14),
        [MembershipType.Premium] = (10, 21),
        [MembershipType.Student] = (3, 7)
    };

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (overdue == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.LoanDate <= toDate.Value);
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

    public async Task<LoanResponse?> GetLoanByIdAsync(int id)
    {
        return await db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Id == id)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName, l.LoanDate, l.DueDate,
                l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync(request.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        // Enforce checkout rules
        if (!patron.IsActive)
        {
            throw new InvalidOperationException("Cannot check out books: patron's membership is inactive.");
        }

        if (book.AvailableCopies <= 0)
        {
            throw new InvalidOperationException($"Cannot check out '{book.Title}': no available copies.");
        }

        var unpaidFines = await db.Fines.Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);
        if (unpaidFines >= 10.00m)
        {
            throw new InvalidOperationException($"Cannot check out books: patron has ${unpaidFines:F2} in unpaid fines (limit is $10.00).");
        }

        var limits = BorrowingLimits[patron.MembershipType];
        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        if (activeLoans >= limits.MaxLoans)
        {
            throw new InvalidOperationException($"Cannot check out books: patron has reached the maximum of {limits.MaxLoans} active loans for {patron.MembershipType} membership.");
        }

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(limits.LoanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;

        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        logger.LogInformation("Book '{Title}' (ID: {BookId}) checked out to patron {PatronId}. Due: {DueDate:yyyy-MM-dd}",
            book.Title, book.Id, patron.Id, loan.DueDate);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<LoanResponse> ReturnBookAsync(int loanId)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new KeyNotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status == LoanStatus.Returned)
        {
            throw new InvalidOperationException("This book has already been returned.");
        }

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Generate fine if overdue
        if (now > loan.DueDate)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day(s) late"
            };

            db.Fines.Add(fine);
            logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId} ({Days} days late)",
                fineAmount, loan.PatronId, loan.Id, overdueDays);
        }

        // Check for pending reservations for this book
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            loan.Book.AvailableCopies--; // Hold the copy for the reservation

            logger.LogInformation("Reservation {ReservationId} for book '{Title}' is now Ready. Expires: {ExpirationDate:yyyy-MM-dd}",
                nextReservation.Id, loan.Book.Title, nextReservation.ExpirationDate);
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Book '{Title}' (ID: {BookId}) returned by patron {PatronId}",
            loan.Book.Title, loan.BookId, loan.PatronId);

        return (await GetLoanByIdAsync(loanId))!;
    }

    public async Task<LoanResponse> RenewLoanAsync(int loanId)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new KeyNotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status != LoanStatus.Active)
        {
            throw new InvalidOperationException("Only active loans can be renewed.");
        }

        if (loan.DueDate < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot renew an overdue loan. Please return the book first.");
        }

        if (loan.RenewalCount >= 2)
        {
            throw new InvalidOperationException("This loan has already been renewed the maximum of 2 times.");
        }

        // Check for unpaid fines
        var unpaidFines = await db.Fines.Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);
        if (unpaidFines >= 10.00m)
        {
            throw new InvalidOperationException($"Cannot renew: patron has ${unpaidFines:F2} in unpaid fines (limit is $10.00).");
        }

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);
        if (hasPendingReservations)
        {
            throw new InvalidOperationException("Cannot renew: there are pending reservations for this book.");
        }

        var limits = BorrowingLimits[loan.Patron.MembershipType];
        loan.DueDate = DateTime.UtcNow.AddDays(limits.LoanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync();

        logger.LogInformation("Loan {LoanId} renewed (count: {RenewalCount}). New due date: {DueDate:yyyy-MM-dd}",
            loanId, loan.RenewalCount, loan.DueDate);

        return (await GetLoanByIdAsync(loanId))!;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Update overdue status
        var overdueLoans = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueLoans.Count > 0)
        {
            await db.SaveChangesAsync();
        }

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName, l.LoanDate, l.DueDate,
                l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }
}
