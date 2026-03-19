using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }

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

    public async Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(BookService.MapLoanToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<LoanDto?> GetLoanByIdAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);
        return loan == null ? null : BookService.MapLoanToDto(loan);
    }

    public async Task<(LoanDto? Loan, string? Error)> CheckoutBookAsync(CreateLoanDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId);
        if (book == null) return (null, "Book not found.");

        var patron = await _db.Patrons.FindAsync(dto.PatronId);
        if (patron == null) return (null, "Patron not found.");

        if (!patron.IsActive)
            return (null, "Patron membership is inactive.");

        if (book.AvailableCopies <= 0)
            return (null, "No available copies of this book.");

        var unpaidFines = await _db.Fines.Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0m;
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (limit $10.00). Please pay outstanding fines before checking out.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxActiveLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            return (null, $"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var loanPeriod = GetLoanPeriodDays(patron.MembershipType);
        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriod),
            Status = LoanStatus.Active,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book checked out: Loan {LoanId}, Book {BookId} to Patron {PatronId}", loan.Id, book.Id, patron.Id);

        // Reload with navigation properties
        var result = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id);
        return (BookService.MapLoanToDto(result), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> ReturnBookAsync(int loanId)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null) return (null, "Loan not found.");
        if (loan.Status == LoanStatus.Returned) return (null, "This loan has already been returned.");

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
            _db.Fines.Add(fine);
            _logger.LogInformation("Fine issued: ${Amount} for Loan {LoanId}, {Days} days overdue", fineAmount, loan.Id, overdueDays);
        }

        // Process pending reservations for this book
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            _logger.LogInformation("Reservation {ResId} marked Ready for Book {BookId}", nextReservation.Id, loan.BookId);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Book returned: Loan {LoanId}, Book {BookId}", loan.Id, loan.BookId);
        return (BookService.MapLoanToDto(loan), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> RenewLoanAsync(int loanId)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null) return (null, "Loan not found.");

        if (loan.Status != LoanStatus.Active)
            return (null, "Only active loans can be renewed.");

        if (loan.DueDate < DateTime.UtcNow)
            return (null, "Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            return (null, "Maximum of 2 renewals has been reached.");

        // Check unpaid fines
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0m;
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Cannot renew.");

        // Check pending reservations
        var hasPendingReservations = await _db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);
        if (hasPendingReservations)
            return (null, "Cannot renew — there are pending reservations for this book.");

        var loanPeriod = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriod);
        loan.RenewalCount++;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan renewed: {LoanId}, Renewal #{Count}", loan.Id, loan.RenewalCount);
        return (BookService.MapLoanToDto(loan), null);
    }

    public async Task<PagedResult<LoanDto>> GetOverdueLoansAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Flag overdue loans
        var overdueLoans = await _db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Any())
            await _db.SaveChangesAsync();

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue && l.ReturnDate == null)
            .OrderBy(l => l.DueDate);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(BookService.MapLoanToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    internal static FineDto MapFineToDto(Fine f) => new()
    {
        Id = f.Id, PatronId = f.PatronId,
        PatronName = $"{f.Patron.FirstName} {f.Patron.LastName}",
        LoanId = f.LoanId, Amount = f.Amount, Reason = f.Reason,
        IssuedDate = f.IssuedDate, PaidDate = f.PaidDate,
        Status = f.Status, CreatedAt = f.CreatedAt
    };
}
