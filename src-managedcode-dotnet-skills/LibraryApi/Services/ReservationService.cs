using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService(LibraryDbContext context, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PagedResult<ReservationDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Reservations.CountAsync();
        var items = await context.Reservations
            .AsNoTracking()
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PagedResult<ReservationDto>(items, totalCount, page, pageSize);
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        return await context.Reservations
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        var book = await context.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

        var patron = await context.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {dto.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron membership is not active.");

        // Patron can't reserve a book they have on active loan
        var hasActiveLoan = await context.Loans
            .AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId &&
                          (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

        if (hasActiveLoan)
            throw new InvalidOperationException("Patron already has an active loan for this book.");

        // Check if patron already has pending/ready reservation for this book
        var hasExistingReservation = await context.Reservations
            .AnyAsync(r => r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasExistingReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // FIFO queue position
        var maxPosition = await context.Reservations
            .Where(r => r.BookId == dto.BookId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = now,
            ExpirationDate = now.AddDays(14),
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        logger.LogInformation("Created reservation {ReservationId} for Book {BookId} by Patron {PatronId}, Queue position: {Position}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        return new ReservationDto(reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition, reservation.CreatedAt);
    }

    public async Task<ReservationDto> CancelAsync(int id)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel reservation with status '{reservation.Status}'.");

        // If it was Ready, release the held copy
        if (reservation.Status == ReservationStatus.Ready)
        {
            reservation.Book.AvailableCopies++;
            reservation.Book.UpdatedAt = DateTime.UtcNow;
        }

        reservation.Status = ReservationStatus.Cancelled;

        await context.SaveChangesAsync();

        logger.LogInformation("Cancelled reservation {ReservationId}", id);

        return new ReservationDto(reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition, reservation.CreatedAt);
    }

    public async Task<ReservationDto> FulfillAsync(int id)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        reservation.Status = ReservationStatus.Fulfilled;

        await context.SaveChangesAsync();

        logger.LogInformation("Fulfilled reservation {ReservationId}", id);

        return new ReservationDto(reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status, reservation.QueuePosition, reservation.CreatedAt);
    }
}
