using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize);
    Task<ReservationResponse?> GetByIdAsync(int id);
    Task<(ReservationResponse? Reservation, string? Error)> CreateAsync(CreateReservationRequest request);
    Task<(ReservationResponse? Reservation, string? Error)> CancelAsync(int id);
    Task<(LoanResponse? Loan, string? Error)> FulfillAsync(int id);
}

public class ReservationService(LibraryDbContext db, ILoanService loanService, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PagedResult<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize)
    {
        var query = db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();

        return new PagedResult<ReservationResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id)
    {
        return await db.Reservations
            .Where(r => r.Id == id)
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .FirstOrDefaultAsync();
    }

    public async Task<(ReservationResponse? Reservation, string? Error)> CreateAsync(CreateReservationRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync(request.PatronId);
        if (patron is null) return (null, "Patron not found.");

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await db.Loans.AnyAsync(l =>
            l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active);
        if (hasActiveLoan)
            return (null, "Patron already has this book on an active loan.");

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await db.Reservations.AnyAsync(r =>
            r.BookId == request.BookId && r.PatronId == request.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasExistingReservation)
            return (null, "Patron already has an active reservation for this book.");

        var maxQueuePosition = await db.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .Select(r => (int?)r.QueuePosition)
            .MaxAsync() ?? 0;

        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            QueuePosition = maxQueuePosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        logger.LogInformation("Reservation {ReservationId} created for book {BookId} by patron {PatronId}", reservation.Id, book.Id, patron.Id);

        return (new ReservationResponse(
            reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            patron.FirstName + " " + patron.LastName,
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition), null);
    }

    public async Task<(ReservationResponse? Reservation, string? Error)> CancelAsync(int id)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null) return (null, "Reservation not found.");
        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Ready))
            return (null, "Only pending or ready reservations can be cancelled.");

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync();

        // Reorder queue positions for remaining reservations
        var remaining = await db.Reservations
            .Where(r => r.BookId == reservation.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        for (int i = 0; i < remaining.Count; i++)
            remaining[i].QueuePosition = i + 1;

        await db.SaveChangesAsync();

        return (new ReservationResponse(
            reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            reservation.Patron.FirstName + " " + reservation.Patron.LastName,
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition), null);
    }

    public async Task<(LoanResponse? Loan, string? Error)> FulfillAsync(int id)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null) return (null, "Reservation not found.");
        if (reservation.Status != ReservationStatus.Ready)
            return (null, "Only reservations with 'Ready' status can be fulfilled.");

        // Check if expired
        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
        {
            reservation.Status = ReservationStatus.Expired;
            await db.SaveChangesAsync();
            return (null, "This reservation has expired.");
        }

        // Create the loan
        var (loan, error) = await loanService.CheckoutAsync(new CreateLoanRequest(reservation.BookId, reservation.PatronId));
        if (loan is null)
            return (null, $"Could not fulfill reservation: {error}");

        reservation.Status = ReservationStatus.Fulfilled;
        await db.SaveChangesAsync();

        logger.LogInformation("Reservation {ReservationId} fulfilled, loan {LoanId} created", reservation.Id, loan.Id);
        return (loan, null);
    }
}
