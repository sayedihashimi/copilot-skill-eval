using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private static int GetMaxLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    private static int GetLoanPeriodDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = db.Loans.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

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

    public async Task<LoanResponse?> GetLoanByIdAsync(int id)
    {
        return await db.Loans.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status.ToString(), l.RenewalCount, l.CreatedAt
            ))
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
            throw new InvalidOperationException("Patron's membership is not active.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No available copies of this book.");

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFines >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay outstanding fines before checking out.");

        var activeLoansCount = await db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxLoans(patron.MembershipType);

        if (activeLoansCount >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var loanPeriod = GetLoanPeriodDays(patron.MembershipType);
        var now = DateTime.UtcNow;

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriod),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        logger.LogInformation("Book checked out: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}", loan.Id, book.Id, patron.Id);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<LoanResponse> ReturnBookAsync(int id)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This book has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Generate overdue fine if applicable
        if (now > loan.DueDate)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day(s) late",
                IssuedDate = now
            };

            db.Fines.Add(fine);
            logger.LogInformation("Fine issued: PatronId={PatronId}, Amount=${Amount}, Reason={Reason}", loan.PatronId, fineAmount, fine.Reason);
        }

        // Check for pending reservations and promote the first one
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            logger.LogInformation("Reservation ready: ReservationId={ReservationId}, PatronId={PatronId}", nextReservation.Id, nextReservation.PatronId);
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Book returned: LoanId={LoanId}, BookId={BookId}", id, loan.BookId);

        return (await GetLoanByIdAsync(id))!;
    }

    public async Task<LoanResponse> RenewLoanAsync(int id)
    {
        var loan = await db.Loans
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed.");

        if (loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum of 2 renewals has been reached.");

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew this loan because there are pending reservations for this book.");

        // Check fine threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFines >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay outstanding fines before renewing.");

        var loanPeriod = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriod);
        loan.RenewalCount++;

        await db.SaveChangesAsync();

        logger.LogInformation("Loan renewed: LoanId={LoanId}, NewDueDate={DueDate}", id, loan.DueDate);

        return (await GetLoanByIdAsync(id))!;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Update overdue statuses
        var overdueLoans = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Count > 0)
            await db.SaveChangesAsync();

        var query = db.Loans.AsNoTracking()
            .Where(l => l.Status == LoanStatus.Overdue && l.ReturnDate == null)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync();
        var items = await query
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
}
