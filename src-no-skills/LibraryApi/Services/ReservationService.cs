using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _db;
    private readonly ILoanService _loanService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext db, ILoanService loanService, ILogger<ReservationService> logger)
    {
        _db = db;
        _loanService = loanService;
        _logger = logger;
    }

    public async Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, int page, int pageSize)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(BookService.MapReservationToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var r = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? null : BookService.MapReservationToDto(r);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CreateReservationAsync(CreateReservationDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId);
        if (book == null) return (null, "Book not found.");

        var patron = await _db.Patrons.FindAsync(dto.PatronId);
        if (patron == null) return (null, "Patron not found.");

        // Patron cannot reserve a book they already have on active loan
        var hasActiveLoan = await _db.Loans.AnyAsync(l =>
            l.BookId == dto.BookId && l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoan)
            return (null, "Patron already has an active loan for this book.");

        // Determine queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation created: {ResId} for Book {BookId} by Patron {PatronId}, Queue #{Pos}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        var result = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstAsync(r => r.Id == reservation.Id);
        return (BookService.MapReservationToDto(result), null);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CancelReservationAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return (null, "Reservation not found.");

        if (reservation.Status == ReservationStatus.Fulfilled || reservation.Status == ReservationStatus.Cancelled)
            return (null, $"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation cancelled: {ResId}", id);
        return (BookService.MapReservationToDto(reservation), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> FulfillReservationAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return (null, "Reservation not found.");

        if (reservation.Status != ReservationStatus.Ready)
            return (null, "Only reservations with 'Ready' status can be fulfilled.");

        // Create the loan via checkout
        var (loan, error) = await _loanService.CheckoutBookAsync(new CreateLoanDto
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        if (loan == null)
            return (null, $"Could not fulfill reservation: {error}");

        reservation.Status = ReservationStatus.Fulfilled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation fulfilled: {ResId}, Loan {LoanId} created", id, loan.Id);
        return (loan, null);
    }
}
