using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var resStatus))
            query = query.Where(r => r.Status == resStatus);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(int id, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.Id == id)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(ReservationResponse? Reservation, string? Error)> CreateReservationAsync(
        CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null) return (null, "Patron not found.");

        // Cannot reserve a book they already have on active loan
        var hasActiveLoan = await db.Loans
            .AnyAsync(l => l.BookId == request.BookId && l.PatronId == request.PatronId &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (hasActiveLoan)
            return (null, "Patron already has this book on an active loan.");

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

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

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation created: {Id} for BookId={BookId}, PatronId={PatronId}", reservation.Id, book.Id, patron.Id);

        return (new ReservationResponse(reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition, reservation.CreatedAt), null);
    }

    public async Task<(ReservationResponse? Reservation, string? Error, bool NotFound)> CancelReservationAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null) return (null, null, true);

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            return (null, $"Cannot cancel a reservation with status '{reservation.Status}'.", false);

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation cancelled: {Id}", id);

        // If it was Ready, move the next pending reservation to Ready
        if (reservation.Status == ReservationStatus.Cancelled)
        {
            var nextPending = await db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync(ct);

            if (nextPending is not null)
            {
                nextPending.Status = ReservationStatus.Ready;
                nextPending.ExpirationDate = DateTime.UtcNow.AddDays(3);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Reservation {Id} moved to Ready status", nextPending.Id);
            }
        }

        return (new ReservationResponse(reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition, reservation.CreatedAt), null, false);
    }

    public async Task<(LoanResponse? Loan, string? Error, bool NotFound)> FulfillReservationAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null) return (null, null, true);

        if (reservation.Status != ReservationStatus.Ready)
            return (null, $"Only reservations with 'Ready' status can be fulfilled. Current status: '{reservation.Status}'.", false);

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            return (null, "This reservation has expired.", false);

        var book = reservation.Book;
        var patron = reservation.Patron;

        if (book.AvailableCopies <= 0)
            return (null, "No available copies of this book.", false);

        // Create loan
        var now = DateTime.UtcNow;
        var loanPeriod = patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriod),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;
        reservation.Status = ReservationStatus.Fulfilled;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, Loan {LoanId} created", id, loan.Id);

        return (new LoanResponse(loan.Id, loan.BookId, book.Title, loan.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt), null, false);
    }
}
