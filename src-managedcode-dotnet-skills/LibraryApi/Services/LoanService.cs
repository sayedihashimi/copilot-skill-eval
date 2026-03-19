using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService(LibraryDbContext context, ILogger<LoanService> logger) : ILoanService
{
    private static readonly Dictionary<MembershipType, (int MaxLoans, int LoanDays)> BorrowingLimits = new()
    {
        [MembershipType.Standard] = (5, 14),
        [MembershipType.Premium] = (10, 21),
        [MembershipType.Student] = (3, 7),
    };

    public async Task<PagedResult<LoanDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Loans.CountAsync();
        var items = await context.Loans
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PagedResult<LoanDto>(items, totalCount, page, pageSize);
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        return await context.Loans
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<LoanDto> CheckoutAsync(CreateLoanDto dto)
    {
        var book = await context.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

        var patron = await context.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {dto.PatronId} not found.");

        // Active membership required
        if (!patron.IsActive)
            throw new InvalidOperationException("Patron membership is not active.");

        // Check available copies
        if (book.AvailableCopies < 1)
            throw new InvalidOperationException("No available copies of this book.");

        // Check unpaid fines threshold ($10)
        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFines >= 10m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to checkout.");

        // Check borrowing limit
        var limits = BorrowingLimits[patron.MembershipType];
        var activeLoans = await context.Loans
            .CountAsync(l => l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

        if (activeLoans >= limits.MaxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum of {limits.MaxLoans} active loans for {patron.MembershipType} membership.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(limits.LoanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        logger.LogInformation("Checkout: Book {BookId} loaned to Patron {PatronId}, Loan {LoanId}, Due {DueDate}",
            dto.BookId, dto.PatronId, loan.Id, loan.DueDate);

        return new LoanDto(loan.Id, loan.BookId, book.Title, loan.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<LoanDto> ReturnAsync(int id)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Auto-fine for overdue: $0.25/day
        if (loan.DueDate < now)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day(s) late at $0.25/day",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            context.Fines.Add(fine);

            logger.LogInformation("Auto-fine of ${Amount:F2} issued for Loan {LoanId} ({Days} days overdue)",
                fineAmount, id, overdueDays);
        }

        // Promote first pending reservation to Ready
        var nextReservation = await context.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);

            // Reserve the copy
            loan.Book.AvailableCopies--;

            logger.LogInformation("Reservation {ReservationId} promoted to Ready for Book {BookId}",
                nextReservation.Id, loan.BookId);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Return: Loan {LoanId} returned by Patron {PatronId}", id, loan.PatronId);

        return new LoanDto(loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<LoanDto> RenewAsync(int id)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum renewal limit (2) reached.");

        // Check if overdue
        if (loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Overdue loans cannot be renewed.");

        // Check unpaid fines threshold
        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFines >= 10m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to renew.");

        // Check reservations on this book
        var hasReservations = await context.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasReservations)
            throw new InvalidOperationException("Cannot renew: there are pending reservations for this book.");

        var limits = BorrowingLimits[loan.Patron.MembershipType];
        var now = DateTime.UtcNow;

        loan.DueDate = now.AddDays(limits.LoanDays);
        loan.RenewalCount++;

        await context.SaveChangesAsync();

        logger.LogInformation("Renewal: Loan {LoanId} renewed (count: {RenewalCount}), new due date: {DueDate}",
            id, loan.RenewalCount, loan.DueDate);

        return new LoanDto(loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<PagedResult<LoanDto>> GetOverdueAsync(int page, int pageSize)
    {
        // Flag overdue loans
        var now = DateTime.UtcNow;
        var overdueLoans = await context.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueLoans.Count > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Flagged {Count} overdue loans", overdueLoans.Count);
        }

        var query = context.Loans.AsNoTracking().Where(l => l.Status == LoanStatus.Overdue);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(l => l.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PagedResult<LoanDto>(items, totalCount, page, pageSize);
    }
}
