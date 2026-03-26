using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger)
    : IReservationService
{
    private static readonly Dictionary<MembershipType, int> LoanDays = new()
    {
        [MembershipType.Standard] = 14,
        [MembershipType.Premium] = 21,
        [MembershipType.Student] = 7
    };

    public async Task<PaginatedResponse<ReservationResponse>> GetAllAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ReservationStatus>(status, true, out var reservationStatus))
            query = query.Where(r => r.Status == reservationStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(ReservationServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<ReservationResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return reservation is null ? null : ReservationServiceHelper.MapToResponse(reservation);
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new ArgumentException("Patron's membership is not active.");

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await db.Loans
            .AnyAsync(l => l.BookId == request.BookId &&
                l.PatronId == request.PatronId &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (hasActiveLoan)
            throw new ArgumentException("Patron already has an active loan for this book and cannot reserve it.");

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await db.Reservations
            .AnyAsync(r => r.BookId == request.BookId &&
                r.PatronId == request.PatronId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasExistingReservation)
            throw new ArgumentException("Patron already has an active reservation for this book.");

        // Get the next queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
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

        logger.LogInformation(
            "Reservation {ReservationId} created for book '{BookTitle}' by patron {PatronId}. Queue position: {Position}",
            reservation.Id, book.Title, patron.Id, reservation.QueuePosition);

        // Reload with navigation
        var created = await db.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstAsync(r => r.Id == reservation.Id, ct);

        return ReservationServiceHelper.MapToResponse(created);
    }

    public async Task<ReservationResponse> CancelAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new ArgumentException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        var wasReady = reservation.Status == ReservationStatus.Ready;
        reservation.Status = ReservationStatus.Cancelled;

        // If this was a Ready reservation, release the held copy and move next in queue
        if (wasReady)
        {
            reservation.Book.AvailableCopies++;
            reservation.Book.UpdatedAt = DateTime.UtcNow;

            // Move next pending reservation to Ready
            var nextReservation = await db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync(ct);

            if (nextReservation is not null)
            {
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
                reservation.Book.AvailableCopies--;

                logger.LogInformation(
                    "Reservation {ReservationId} moved to Ready after cancellation of {CancelledId}",
                    nextReservation.Id, id);
            }
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} cancelled", id);

        return ReservationServiceHelper.MapToResponse(reservation);
    }

    public async Task<LoanResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new ArgumentException($"Only reservations with status 'Ready' can be fulfilled. Current status: '{reservation.Status}'.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            throw new ArgumentException("This reservation has expired.");

        var patron = reservation.Patron;
        var book = reservation.Book;
        var loanDays = LoanDays[patron.MembershipType];

        var now = DateTime.UtcNow;

        // Create the loan (the copy was already held when reservation became Ready)
        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        reservation.Status = ReservationStatus.Fulfilled;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reservation {ReservationId} fulfilled. Loan {LoanId} created for book '{BookTitle}' to patron {PatronId}",
            id, loan.Id, book.Title, patron.Id);

        // Reload loan with navigation
        var created = await db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id, ct);

        return LoanServiceHelper.MapToResponse(created);
    }
}
