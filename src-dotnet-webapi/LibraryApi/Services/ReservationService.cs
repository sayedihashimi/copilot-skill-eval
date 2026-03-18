using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService(LibraryDbContext db) : IReservationService
{
    public async Task<PagedResponse<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        query = query.OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Paginate(items.Select(MapToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return reservation is null ? null : MapToResponse(reservation);
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron membership is not active.");

        // Can't reserve a book the patron has on active loan
        var hasActiveLoan = await db.Loans
            .AnyAsync(l => l.BookId == book.Id && l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (hasActiveLoan)
            throw new InvalidOperationException("Patron already has an active loan for this book.");

        // Can't reserve if already has pending/ready reservation
        var hasExistingReservation = await db.Reservations
            .AnyAsync(r => r.BookId == book.Id && r.PatronId == patron.Id && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasExistingReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Get next queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == book.Id && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = book.Id,
            PatronId = patron.Id,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        await db.Entry(reservation).Reference(r => r.Book).LoadAsync(ct);
        await db.Entry(reservation).Reference(r => r.Patron).LoadAsync(ct);
        return MapToResponse(reservation);
    }

    public async Task CancelAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException($"Cannot cancel reservation with status '{reservation.Status}'.");

        // If it was Ready, release the held copy
        if (reservation.Status == ReservationStatus.Ready)
        {
            var book = await db.Books.FindAsync([reservation.BookId], ct);
            if (book is not null)
            {
                book.AvailableCopies++;
                book.UpdatedAt = DateTime.UtcNow;
            }
        }

        reservation.Status = ReservationStatus.Cancelled;

        // Reorder queue positions
        var subsequentReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId
                && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready)
                && r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in subsequentReservations)
            r.QueuePosition--;

        await db.SaveChangesAsync(ct);
    }

    public async Task<LoanResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            throw new InvalidOperationException("This reservation has expired.");

        var patron = reservation.Patron;
        var book = reservation.Book;
        var now = DateTime.UtcNow;

        // The copy was already reserved (decremented) when status was set to Ready
        var loanDays = patron.MembershipType switch
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
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        reservation.Status = ReservationStatus.Fulfilled;

        // Reorder queue
        var subsequentReservations = await db.Reservations
            .Where(r => r.BookId == book.Id
                && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready)
                && r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in subsequentReservations)
            r.QueuePosition--;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        await db.Entry(loan).Reference(l => l.Book).LoadAsync(ct);
        await db.Entry(loan).Reference(l => l.Patron).LoadAsync(ct);

        return new LoanResponse
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BookISBN = loan.Book.ISBN,
            PatronId = loan.PatronId,
            PatronName = loan.Patron.FirstName + " " + loan.Patron.LastName,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            Status = loan.Status,
            RenewalCount = loan.RenewalCount,
            CreatedAt = loan.CreatedAt
        };
    }

    private static ReservationResponse MapToResponse(Reservation r) => new()
    {
        Id = r.Id,
        BookId = r.BookId,
        BookTitle = r.Book.Title,
        PatronId = r.PatronId,
        PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
        ReservationDate = r.ReservationDate,
        ExpirationDate = r.ExpirationDate,
        Status = r.Status,
        QueuePosition = r.QueuePosition,
        CreatedAt = r.CreatedAt
    };

    private static PagedResponse<T> Paginate<T>(List<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResponse<T>
        {
            Items = items, Page = page, PageSize = pageSize,
            TotalCount = totalCount, TotalPages = totalPages,
            HasNextPage = page < totalPages, HasPreviousPage = page > 1
        };
    }
}
