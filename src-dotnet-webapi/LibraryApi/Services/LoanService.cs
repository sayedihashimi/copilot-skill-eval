using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private static int GetLoanPeriodDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    private static int GetMaxActiveLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<LoanResponse?> GetLoanByIdAsync(int id, CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Id == id)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(LoanResponse? Loan, string? Error)> CheckoutBookAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null) return (null, "Patron not found.");

        if (!patron.IsActive)
            return (null, "Patron's membership is not active.");

        if (book.AvailableCopies <= 0)
            return (null, "No available copies of this book.");

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (limit is $10.00). Please pay fines before checking out.");

        var activeLoans = await db.Loans
            .CountAsync(l => l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        var maxLoans = GetMaxActiveLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            return (null, $"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var loanPeriod = GetLoanPeriodDays(patron.MembershipType);
        var now = DateTime.UtcNow;

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriod),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Book checked out: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}", loan.Id, book.Id, patron.Id);

        return (new LoanResponse(loan.Id, loan.BookId, book.Title, loan.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt), null);
    }

    public async Task<(LoanResponse? Loan, string? Error, bool NotFound)> ReturnBookAsync(int loanId, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null) return (null, null, true);

        if (loan.Status == LoanStatus.Returned)
            return (null, "This loan has already been returned.", false);

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Check for overdue fine
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
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };

            db.Fines.Add(fine);
            logger.LogInformation("Fine issued: PatronId={PatronId}, Amount=${Amount}, Reason={Reason}", loan.PatronId, fineAmount, fine.Reason);
        }

        await db.SaveChangesAsync(ct);

        // Check for pending reservations
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Reservation {ReservationId} moved to Ready status for BookId={BookId}", nextReservation.Id, loan.BookId);
        }

        logger.LogInformation("Book returned: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}", loan.Id, loan.BookId, loan.PatronId);

        return (new LoanResponse(loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt), null, false);
    }

    public async Task<(LoanResponse? Loan, string? Error, bool NotFound)> RenewLoanAsync(int loanId, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null) return (null, null, true);

        if (loan.Status == LoanStatus.Returned)
            return (null, "Cannot renew a returned loan.", false);

        if (loan.Status == LoanStatus.Overdue || (loan.DueDate < DateTime.UtcNow && loan.ReturnDate is null))
            return (null, "Cannot renew an overdue loan.", false);

        if (loan.RenewalCount >= 2)
            return (null, "Maximum renewal limit of 2 has been reached.", false);

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Please pay fines before renewing.", false);

        // Check pending reservations
        var hasReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending, ct);

        if (hasReservations)
            return (null, "Cannot renew — there are pending reservations for this book.", false);

        var loanPeriod = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriod);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Loan renewed: LoanId={LoanId}, NewDueDate={DueDate}, RenewalCount={Count}", loan.Id, loan.DueDate, loan.RenewalCount);

        return (new LoanResponse(loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt), null, false);
    }

    public async Task<IReadOnlyList<LoanResponse>> GetOverdueLoansAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Also update any active loans that are past due
        var newlyOverdue = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in newlyOverdue)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (newlyOverdue.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("{Count} loans flagged as overdue", newlyOverdue.Count);
        }

        return await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue && l.ReturnDate == null)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);
    }
}
