using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public class ReservationService(LibraryDbContext db, ILoanService loanService, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize)
    {
        var query = db.Reservations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status.ToString(), r.QueuePosition, r.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status.ToString(), r.QueuePosition, r.CreatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync(request.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron's membership is not active.");

        // A patron cannot reserve a book they already have on active loan
        var hasActiveLoan = await db.Loans
            .AnyAsync(l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active);

        if (hasActiveLoan)
            throw new InvalidOperationException("Patron already has an active loan for this book.");

        // Check for existing active reservation
        var hasActiveReservation = await db.Reservations
            .AnyAsync(r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasActiveReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            QueuePosition = maxPosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        logger.LogInformation("Reservation created: ReservationId={ReservationId}, BookId={BookId}, PatronId={PatronId}, QueuePosition={Position}",
            reservation.Id, book.Id, patron.Id, reservation.QueuePosition);

        return (await GetReservationByIdAsync(reservation.Id))!;
    }

    public async Task<ReservationResponse> CancelReservationAsync(int id)
    {
        var reservation = await db.Reservations.FindAsync(id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;

        // If this was a Ready reservation, promote the next Pending one
        if (reservation.Status == ReservationStatus.Ready)
        {
            var nextReservation = await db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation is not null)
            {
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
            }
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Reservation cancelled: ReservationId={ReservationId}", id);

        return (await GetReservationByIdAsync(id))!;
    }

    public async Task<LoanResponse> FulfillReservationAsync(int id)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            throw new InvalidOperationException("This reservation has expired.");

        reservation.Status = ReservationStatus.Fulfilled;

        await db.SaveChangesAsync();

        // Create a loan for the patron
        var loan = await loanService.CheckoutBookAsync(new CreateLoanRequest(reservation.BookId, reservation.PatronId));

        logger.LogInformation("Reservation fulfilled: ReservationId={ReservationId}, LoanId={LoanId}", id, loan.Id);

        return loan;
    }
}
