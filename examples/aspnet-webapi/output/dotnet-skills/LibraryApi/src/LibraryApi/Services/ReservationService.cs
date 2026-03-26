using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext context, ILoanService loanService, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PagedResult<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize)
    {
        var query = context.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => BookService.MapReservationResponse(r))
            .ToListAsync();

        return new PagedResult<ReservationResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        return await context.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.Id == id)
            .Select(r => BookService.MapReservationResponse(r))
            .FirstOrDefaultAsync();
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
    {
        var book = await context.Books.FindAsync(request.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await context.Patrons.FindAsync(request.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        // Cannot reserve a book the patron already has on active loan
        var hasActiveLoan = await context.Loans
            .AnyAsync(l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active);

        if (hasActiveLoan)
            throw new InvalidOperationException("Patron already has this book on an active loan and cannot reserve it.");

        // Already has a pending/ready reservation for this book
        var hasExistingReservation = await context.Reservations
            .AnyAsync(r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasExistingReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await context.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        logger.LogInformation("Reservation {ReservationId} created for book {BookId} by patron {PatronId} at queue position {Position}",
            reservation.Id, book.Id, patron.Id, reservation.QueuePosition);

        return (await GetReservationByIdAsync(reservation.Id))!;
    }

    public async Task<ReservationResponse> CancelReservationAsync(int id)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        var wasReady = reservation.Status == ReservationStatus.Ready;
        reservation.Status = ReservationStatus.Cancelled;

        // If it was Ready, move next Pending reservation to Ready
        if (wasReady)
        {
            var nextReservation = await context.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation is not null)
            {
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
                logger.LogInformation("Reservation {Id} moved to Ready after cancellation of {CancelledId}", nextReservation.Id, id);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Reservation {ReservationId} cancelled", id);

        return (await GetReservationByIdAsync(id))!;
    }

    public async Task<LoanResponse> FulfillReservationAsync(int id)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException($"Only 'Ready' reservations can be fulfilled. Current status: '{reservation.Status}'.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            throw new InvalidOperationException("This reservation has expired.");

        // Create a loan via the loan service
        var loanResponse = await loanService.CheckoutBookAsync(new CreateLoanRequest
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        reservation.Status = ReservationStatus.Fulfilled;
        await context.SaveChangesAsync();

        logger.LogInformation("Reservation {ReservationId} fulfilled, loan {LoanId} created", id, loanResponse.Id);

        return loanResponse;
    }
}
