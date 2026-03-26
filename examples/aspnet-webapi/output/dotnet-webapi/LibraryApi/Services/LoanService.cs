using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    : ILoanService
{
    private static readonly Dictionary<MembershipType, (int MaxLoans, int LoanDays)> BorrowingLimits = new()
    {
        [MembershipType.Standard] = (5, 14),
        [MembershipType.Premium] = (10, 21),
        [MembershipType.Student] = (3, 7)
    };

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue ||
                (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(LoanServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<LoanResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        return loan is null ? null : LoanServiceHelper.MapToResponse(loan);
    }

    public async Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        // Enforce checkout rules
        if (!patron.IsActive)
            throw new ArgumentException("Patron's membership is not active.");

        if (book.AvailableCopies < 1)
            throw new ArgumentException($"No available copies of '{book.Title}'.");

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
            throw new ArgumentException(
                $"Patron has ${unpaidFines:F2} in unpaid fines (threshold is $10.00). Please pay fines before checking out.");

        var limits = BorrowingLimits[patron.MembershipType];
        var activeLoans = await db.Loans
            .CountAsync(l => l.PatronId == patron.Id &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (activeLoans >= limits.MaxLoans)
            throw new ArgumentException(
                $"Patron has reached the borrowing limit of {limits.MaxLoans} active loans for {patron.MembershipType} membership.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(limits.LoanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Book '{BookTitle}' checked out to patron {PatronId} ({PatronName}). Loan {LoanId}, due {DueDate}",
            book.Title, patron.Id, $"{patron.FirstName} {patron.LastName}", loan.Id, loan.DueDate);

        // Reload with navigation properties
        var created = await db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id, ct);

        return LoanServiceHelper.MapToResponse(created);
    }

    public async Task<LoanResponse> ReturnAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new ArgumentException("This loan has already been returned.");

        var now = DateTime.UtcNow;

        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Check for overdue and generate fine
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
            logger.LogInformation(
                "Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId} ({Days} days late)",
                fineAmount, loan.PatronId, loan.Id, overdueDays);
        }

        // Check for pending reservations
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);

            // The returned copy is now "reserved" so decrement available again
            loan.Book.AvailableCopies--;

            logger.LogInformation(
                "Reservation {ReservationId} for book '{BookTitle}' moved to Ready status. Patron has until {ExpirationDate} to pick up.",
                nextReservation.Id, loan.Book.Title, nextReservation.ExpirationDate);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Book '{BookTitle}' returned. Loan {LoanId} completed.", loan.Book.Title, loan.Id);

        return LoanServiceHelper.MapToResponse(loan);
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new ArgumentException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue || loan.DueDate < DateTime.UtcNow)
            throw new ArgumentException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            throw new ArgumentException("Maximum renewal limit (2) has been reached.");

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending, ct);

        if (hasPendingReservations)
            throw new ArgumentException("Cannot renew this loan because there are pending reservations for the book.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
            throw new ArgumentException(
                $"Patron has ${unpaidFines:F2} in unpaid fines (threshold is $10.00). Please pay fines before renewing.");

        var limits = BorrowingLimits[loan.Patron.MembershipType];
        loan.DueDate = DateTime.UtcNow.AddDays(limits.LoanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Loan {LoanId} renewed (renewal #{Count}). New due date: {DueDate}",
            loan.Id, loan.RenewalCount, loan.DueDate);

        return LoanServiceHelper.MapToResponse(loan);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetOverdueAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Update overdue statuses
        var overdueLoans = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Count > 0)
            await db.SaveChangesAsync(ct);

        var query = db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(LoanServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<LoanResponse>.Create(responses, page, pageSize, totalCount);
    }
}
