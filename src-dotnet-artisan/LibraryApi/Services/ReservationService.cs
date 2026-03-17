using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db) : IReservationService
{
    public async Task<PaginatedResponse<ReservationDto>> GetAllAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);

        return new PaginatedResponse<ReservationDto>(items, total, page, pageSize);
    }

    public async Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct);
        if (book is null) return (null, "Book not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null) return (null, "Patron not found.");

        if (!patron.IsActive) return (null, "Patron account is inactive.");

        // Cannot reserve a book already on active loan by same patron
        var hasActiveLoan = await db.Loans.AnyAsync(
            l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status != LoanStatus.Returned, ct);
        if (hasActiveLoan)
            return (null, "Cannot reserve a book that is currently on active loan by the same patron.");

        // Check for existing active reservation by same patron
        var hasActiveReservation = await db.Reservations.AnyAsync(
            r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                 (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasActiveReservation)
            return (null, "Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            ReservationDate = DateTime.UtcNow,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.Id, ct), null);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CancelAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([id], ct);
        if (reservation is null) return (null, "Reservation not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            return (null, $"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null) return (null, "Reservation not found.");

        if (reservation.Status != ReservationStatus.Ready)
            return (null, "Only reservations with 'Ready' status can be fulfilled.");

        if (reservation.Book.AvailableCopies <= 0)
            return (null, "No copies available to fulfill this reservation.");

        // Create loan
        var loanDays = reservation.Patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;
        reservation.Book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        var loanDto = await db.Loans.AsNoTracking()
            .Where(l => l.Id == loan.Id)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .FirstAsync(ct);

        return (loanDto, null);
    }
}
