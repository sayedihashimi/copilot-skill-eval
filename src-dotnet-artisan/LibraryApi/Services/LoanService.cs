using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanResponse>> GetAllAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDetailResponse?> GetByIdAsync(int id);
    Task<(LoanResponse? Loan, string? Error)> CheckoutAsync(CreateLoanRequest request);
    Task<(LoanResponse? Loan, string? Error)> ReturnAsync(int id);
    Task<(LoanResponse? Loan, string? Error)> RenewAsync(int id);
    Task<List<LoanResponse>> GetOverdueAsync();
}

public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private static int GetMaxLoans(MembershipType type) => type switch
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

    public async Task<PagedResult<LoanResponse>> GetAllAsync(
        LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

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
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();

        return new PagedResult<LoanResponse>(items, totalCount, page, pageSize);
    }

    public async Task<LoanDetailResponse?> GetByIdAsync(int id)
    {
        return await db.Loans
            .Where(l => l.Id == id)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Select(l => new LoanDetailResponse(
                l.Id, l.BookId, l.Book.Title, l.Book.ISBN,
                l.PatronId, l.Patron.FirstName + " " + l.Patron.LastName, l.Patron.Email,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<(LoanResponse? Loan, string? Error)> CheckoutAsync(CreateLoanRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync(request.PatronId);
        if (patron is null) return (null, "Patron not found.");

        if (!patron.IsActive)
            return (null, "Patron membership is not active.");

        if (book.AvailableCopies < 1)
            return (null, "No available copies of this book.");

        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            return (null, $"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to check out books.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        logger.LogInformation("Book {BookId} checked out to patron {PatronId}, due {DueDate}", book.Id, patron.Id, loan.DueDate);

        return (new LoanResponse(
            loan.Id, loan.BookId, book.Title, loan.PatronId,
            patron.FirstName + " " + patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount), null);
    }

    public async Task<(LoanResponse? Loan, string? Error)> ReturnAsync(int id)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null) return (null, "Loan not found.");
        if (loan.Status == LoanStatus.Returned) return (null, "This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Generate fine for overdue returns
        if (now > loan.DueDate)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = "Overdue return",
                IssuedDate = now
            };
            db.Fines.Add(fine);
            logger.LogInformation("Fine of ${Amount} issued to patron {PatronId} for overdue loan {LoanId}", fineAmount, loan.PatronId, loan.Id);
        }

        await db.SaveChangesAsync();

        // Check for pending reservations and transition first to Ready
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            await db.SaveChangesAsync();
            logger.LogInformation("Reservation {ReservationId} moved to Ready status", nextReservation.Id);
        }

        return (new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            loan.Patron.FirstName + " " + loan.Patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount), null);
    }

    public async Task<(LoanResponse? Loan, string? Error)> RenewAsync(int id)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null) return (null, "Loan not found.");
        if (loan.Status == LoanStatus.Returned) return (null, "Cannot renew a returned loan.");
        if (loan.Status == LoanStatus.Overdue || loan.DueDate < DateTime.UtcNow)
            return (null, "Cannot renew an overdue loan.");
        if (loan.RenewalCount >= 2) return (null, "Maximum of 2 renewals reached.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to renew books.");

        // Check for pending reservations on this book
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);
        if (hasPendingReservations)
            return (null, "Cannot renew — there are pending reservations for this book.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync();
        logger.LogInformation("Loan {LoanId} renewed, new due date {DueDate}", loan.Id, loan.DueDate);

        return (new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            loan.Patron.FirstName + " " + loan.Patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount), null);
    }

    public async Task<List<LoanResponse>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;

        // Also flag overdue loans
        var overdueLoans = await db.Loans
            .Where(l => l.DueDate < now && l.ReturnDate == null && l.Status == LoanStatus.Active)
            .ToListAsync();

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Count > 0)
            await db.SaveChangesAsync();

        return await db.Loans
            .Where(l => l.DueDate < now && l.ReturnDate == null)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();
    }
}
