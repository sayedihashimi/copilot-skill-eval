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

    public async Task<PagedResult<LoanDto>> GetAllAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue) query = query.Where(l => l.LoanDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(l => l.LoanDate <= toDate.Value);

        query = query.OrderByDescending(l => l.LoanDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstOrDefaultAsync(l => l.Id == id);
        return loan == null ? null : MapToDto(loan);
    }

    public async Task<LoanDto> CheckoutAsync(LoanCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        // Enforce checkout rules
        if (!patron.IsActive)
            throw new BusinessRuleException("Patron's membership is not active.");
        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("No available copies of this book.");

        var unpaidFines = await _db.Fines.Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0;
        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold is $10.00). Please pay fines before checking out.");

        var activeLoansCount = await _db.Loans.CountAsync(l => l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var maxLoans = GetMaxActiveLoans(patron.MembershipType);
        if (activeLoansCount >= maxLoans)
            throw new BusinessRuleException($"Patron has reached the maximum number of active loans ({maxLoans}) for {patron.MembershipType} membership.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = book.Id, PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(GetLoanPeriodDays(patron.MembershipType)),
            Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book '{Title}' (ID: {BookId}) checked out to patron '{Name}' (ID: {PatronId})", book.Title, book.Id, $"{patron.FirstName} {patron.LastName}", patron.Id);

        var result = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstAsync(l => l.Id == loan.Id);
        return MapToDto(result);
    }

    public async Task<LoanDto> ReturnAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Generate overdue fine if applicable
        if (now > loan.DueDate)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;
            var fine = new Fine
            {
                PatronId = loan.PatronId, LoanId = loan.Id,
                Amount = fineAmount, Reason = "Overdue return",
                IssuedDate = now, Status = FineStatus.Unpaid, CreatedAt = now
            };
            _db.Fines.Add(fine);
            _logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue return of loan {LoanId} ({Days} days overdue)", fineAmount, loan.PatronId, loan.Id, overdueDays);
        }

        // Check for pending reservations
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            _logger.LogInformation("Reservation {ReservationId} for book {BookId} moved to Ready status", nextReservation.Id, loan.BookId);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Book '{Title}' (ID: {BookId}) returned by patron {PatronId}", loan.Book.Title, loan.BookId, loan.PatronId);

        return MapToDto(loan);
    }

    public async Task<LoanDto> RenewAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("Cannot renew a returned loan.");
        if (loan.Status == LoanStatus.Overdue)
            throw new BusinessRuleException("Cannot renew an overdue loan.");
        if (loan.RenewalCount >= 2)
            throw new BusinessRuleException("Maximum number of renewals (2) has been reached.");

        // Check unpaid fines threshold
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0;
        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay fines before renewing.");

        // Check pending reservations
        var hasPendingReservations = await _db.Reservations.AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);
        if (hasPendingReservations)
            throw new BusinessRuleException("Cannot renew this loan because there are pending reservations for this book.");

        var now = DateTime.UtcNow;
        loan.DueDate = now.AddDays(GetLoanPeriodDays(loan.Patron.MembershipType));
        loan.RenewalCount++;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} renewed (renewal #{Count}). New due date: {DueDate}", loan.Id, loan.RenewalCount, loan.DueDate);

        return MapToDto(loan);
    }

    public async Task<PagedResult<LoanDto>> GetOverdueAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Also flag loans that are past due but not yet marked overdue
        var overdueLoans = await _db.Loans
            .Where(l => l.ReturnDate == null && l.DueDate < now && l.Status == LoanStatus.Active)
            .ToListAsync();
        foreach (var l in overdueLoans)
            l.Status = LoanStatus.Overdue;
        if (overdueLoans.Any())
            await _db.SaveChangesAsync();

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    internal static LoanDto MapToDto(Loan l) => new()
    {
        Id = l.Id, BookId = l.BookId, BookTitle = l.Book.Title, BookISBN = l.Book.ISBN,
        PatronId = l.PatronId, PatronName = $"{l.Patron.FirstName} {l.Patron.LastName}",
        LoanDate = l.LoanDate, DueDate = l.DueDate, ReturnDate = l.ReturnDate,
        Status = l.Status.ToString(), RenewalCount = l.RenewalCount, CreatedAt = l.CreatedAt
    };
}
