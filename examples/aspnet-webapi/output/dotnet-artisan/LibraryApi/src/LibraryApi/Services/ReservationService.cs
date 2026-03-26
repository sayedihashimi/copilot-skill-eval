using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db, ILoanService loanService, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(ReservationStatus? status, int page, int pageSize)
    {
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName, r.ReservationDate, r.ExpirationDate,
                r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.Id == id)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName, r.ReservationDate, r.ExpirationDate,
                r.Status, r.QueuePosition, r.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
    {
        var book = await db.Books.FindAsync(request.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync(request.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
        {
            throw new InvalidOperationException("Cannot create reservation: patron's membership is inactive.");
        }

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await db.Loans.AnyAsync(l =>
            l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active);
        if (hasActiveLoan)
        {
            throw new InvalidOperationException("Cannot reserve a book you already have on an active loan.");
        }

        // Check for existing pending/ready reservation by same patron for same book
        var hasExistingReservation = await db.Reservations.AnyAsync(r =>
            r.BookId == request.BookId && r.PatronId == request.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasExistingReservation)
        {
            throw new InvalidOperationException("You already have an active reservation for this book.");
        }

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

        logger.LogInformation("Reservation {ReservationId} created for book '{Title}' by patron {PatronId} at position {Position}",
            reservation.Id, book.Title, patron.Id, reservation.QueuePosition);

        return (await GetReservationByIdAsync(reservation.Id))!;
    }

    public async Task<ReservationResponse> CancelReservationAsync(int id)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
        {
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");
        }

        var wasReady = reservation.Status == ReservationStatus.Ready;
        reservation.Status = ReservationStatus.Cancelled;

        // If cancelled reservation was Ready, make the held copy available again and promote next
        if (wasReady)
        {
            reservation.Book.AvailableCopies++;

            var nextReservation = await db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation is not null)
            {
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
                reservation.Book.AvailableCopies--;

                logger.LogInformation("Reservation {ReservationId} promoted to Ready", nextReservation.Id);
            }
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Reservation {ReservationId} cancelled", id);

        return (await GetReservationByIdAsync(id))!;
    }

    public async Task<LoanResponse> FulfillReservationAsync(int id)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
        {
            throw new InvalidOperationException($"Only reservations with 'Ready' status can be fulfilled. Current status: '{reservation.Status}'.");
        }

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("This reservation has expired.");
        }

        // The copy was already held when it became Ready, so we need to make it available before checkout decrements it
        reservation.Book.AvailableCopies++;
        reservation.Status = ReservationStatus.Fulfilled;

        await db.SaveChangesAsync();

        // Create the loan via the loan service (which will decrement AvailableCopies)
        var loanResponse = await loanService.CheckoutBookAsync(new CreateLoanRequest
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        logger.LogInformation("Reservation {ReservationId} fulfilled. Loan {LoanId} created", id, loanResponse.Id);

        return loanResponse;
    }
}
