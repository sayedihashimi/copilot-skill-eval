using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext context, ILogger<LoanService> logger) : ILoanService
{
    private const decimal FinePerDay = 0.25m;
    private const decimal FineThreshold = 10.00m;
    private const int MaxRenewals = 2;

    public async Task<PagedResult<LoanResponse>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = context.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => BookService.MapLoanResponse(l))
            .ToListAsync();

        return new PagedResult<LoanResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LoanResponse?> GetLoanByIdAsync(int id)
    {
        return await context.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Id == id)
            .Select(l => BookService.MapLoanResponse(l))
            .FirstOrDefaultAsync();
    }

    public async Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request)
    {
        var book = await context.Books.FindAsync(request.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await context.Patrons.FindAsync(request.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        // Enforce checkout rules
        if (!patron.IsActive)
            throw new InvalidOperationException("Patron's membership is not active.");

        if (book.AvailableCopies < 1)
            throw new InvalidOperationException("No available copies of this book.");

        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2}). Fines must be paid before checking out.");

        var activeLoans = await context.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum number of active loans ({maxLoans}) for {patron.MembershipType} membership.");

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

        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}", book.Id, patron.Id, loan.Id);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<LoanResponse> ReturnBookAsync(int loanId)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new KeyNotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;

        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Calculate overdue fine
        if (now > loan.DueDate)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * FinePerDay;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day{(overdueDays != 1 ? "s" : "")} late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };

            context.Fines.Add(fine);
            logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId}", fineAmount, loan.PatronId, loan.Id);
        }

        // Check for pending reservations for this book
        var nextReservation = await context.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            logger.LogInformation("Reservation {ReservationId} for book {BookId} moved to Ready status", nextReservation.Id, loan.BookId);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Book {BookId} returned by patron {PatronId}, loan {LoanId}", loan.BookId, loan.PatronId, loan.Id);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<LoanResponse> RenewLoanAsync(int loanId)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new KeyNotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed.");

        if (loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= MaxRenewals)
            throw new InvalidOperationException($"Maximum number of renewals ({MaxRenewals}) has been reached.");

        var hasPendingReservations = await context.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew this loan because there are pending reservations for the book.");

        // Check fine threshold
        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Fines must be paid before renewing.");

        var loanPeriod = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriod);
        loan.RenewalCount++;

        await context.SaveChangesAsync();
        logger.LogInformation("Loan {LoanId} renewed (renewal {Count}/{Max})", loanId, loan.RenewalCount, MaxRenewals);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<List<LoanResponse>> GetOverdueLoansAsync()
    {
        var now = DateTime.UtcNow;

        // Flag overdue loans
        var overdueLoans = await context.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueLoans.Count > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Flagged {Count} loans as overdue", overdueLoans.Count);
        }

        return await context.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate)
            .Select(l => BookService.MapLoanResponse(l))
            .ToListAsync();
    }

    internal static int GetMaxLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    internal static int GetLoanPeriodDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };
}
