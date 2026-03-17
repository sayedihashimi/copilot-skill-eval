using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db) : ILoanService
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

    public async Task<PaginatedResponse<LoanDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanDto>(items, total, page, pageSize);
    }

    public async Task<LoanDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(LoanDto? Loan, string? Error)> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null) return (null, "Patron not found.");

        if (!patron.IsActive) return (null, "Patron account is inactive.");

        if (book.AvailableCopies <= 0) return (null, "No copies available for checkout.");

        var activeLoans = await db.Loans.CountAsync(
            l => l.PatronId == request.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            return (null, $"Patron has reached the maximum of {maxLoans} active loans.");

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == request.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to checkout.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(loan.Id, ct), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> ReturnAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (loan is null) return (null, "Loan not found.");
        if (loan.Status == LoanStatus.Returned) return (null, "Loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Auto-generate fine if overdue
        if (now > loan.DueDate)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * 0.25m;
            db.Fines.Add(new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue: {daysOverdue} day(s) late on '{loan.Book.Title}'",
                IssuedDate = now,
                Status = FineStatus.Unpaid
            });
        }

        // Check pending reservations for this book → first one transitions to Ready
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(loan.Id, ct), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans.Include(l => l.Patron).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (loan is null) return (null, "Loan not found.");

        if (loan.Status == LoanStatus.Returned) return (null, "Cannot renew a returned loan.");
        if (loan.Status == LoanStatus.Overdue) return (null, "Cannot renew an overdue loan.");
        if (loan.RenewalCount >= 2) return (null, "Maximum renewals (2) reached.");

        // Check fine threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to renew.");

        // Check pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasPendingReservations) return (null, "Cannot renew — there are pending reservations for this book.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(loan.Id, ct), null);
    }

    public async Task<IReadOnlyList<LoanDto>> GetOverdueAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Also update status for newly overdue loans
        var newlyOverdue = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in newlyOverdue)
            loan.Status = LoanStatus.Overdue;

        if (newlyOverdue.Count > 0)
            await db.SaveChangesAsync(ct);

        return await db.Loans.AsNoTracking()
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);
    }
}
