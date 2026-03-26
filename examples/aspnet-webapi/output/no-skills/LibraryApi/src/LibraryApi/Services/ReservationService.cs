using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext db, ILogger<ReservationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ReservationDto>> GetAllAsync(string? status, int page, int pageSize)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        query = query.OrderByDescending(r => r.ReservationDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstOrDefaultAsync(r => r.Id == id);
        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateAsync(ReservationCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        // Check if patron has an active loan for this book
        var hasActiveLoan = await _db.Loans.AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoan)
            throw new BusinessRuleException("Patron already has an active loan for this book and cannot reserve it.");

        // Check for existing pending/ready reservation
        var hasExistingReservation = await _db.Reservations.AnyAsync(r => r.BookId == dto.BookId && r.PatronId == dto.PatronId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasExistingReservation)
            throw new BusinessRuleException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = dto.BookId, PatronId = dto.PatronId,
            ReservationDate = now, Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1, CreatedAt = now
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation created for book {BookId} by patron {PatronId}, queue position {Position}", dto.BookId, dto.PatronId, reservation.QueuePosition);

        var result = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstAsync(r => r.Id == reservation.Id);
        return MapToDto(result);
    }

    public async Task<ReservationDto> CancelAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status == ReservationStatus.Fulfilled || reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a reservation that is already {reservation.Status}.");

        reservation.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} cancelled", id);
        return MapToDto(reservation);
    }

    public async Task<LoanDto> FulfillAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException($"Only reservations with 'Ready' status can be fulfilled. Current status: {reservation.Status}.");

        var patron = reservation.Patron;
        var book = reservation.Book;

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("No available copies of this book.");

        var now = DateTime.UtcNow;

        // Create the loan
        var loanPeriodDays = patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var loan = new Loan
        {
            BookId = book.Id, PatronId = patron.Id,
            LoanDate = now, DueDate = now.AddDays(loanPeriodDays),
            Status = LoanStatus.Active, RenewalCount = 0, CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;
        reservation.Status = ReservationStatus.Fulfilled;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} fulfilled. Loan {LoanId} created for patron {PatronId}", id, loan.Id, patron.Id);

        var result = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstAsync(l => l.Id == loan.Id);
        return LoanService.MapToDto(result);
    }

    internal static ReservationDto MapToDto(Reservation r) => new()
    {
        Id = r.Id, BookId = r.BookId, BookTitle = r.Book.Title,
        PatronId = r.PatronId, PatronName = $"{r.Patron.FirstName} {r.Patron.LastName}",
        ReservationDate = r.ReservationDate, ExpirationDate = r.ExpirationDate,
        Status = r.Status.ToString(), QueuePosition = r.QueuePosition, CreatedAt = r.CreatedAt
    };
}
