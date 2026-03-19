using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<BookService> _logger;

    public BookService(LibraryDbContext db, ILogger<BookService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, int page, int pageSize)
    {
        var query = _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(s) ||
                b.ISBN.ToLower().Contains(s) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(s) || ba.Author.LastName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var c = category.ToLower();
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == c));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "year" => query.OrderByDescending(b => b.PublicationYear),
            "recent" => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            ISBN = dto.ISBN,
            Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear,
            Description = dto.Description,
            PageCount = dto.PageCount,
            Language = dto.Language,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var authorId in dto.AuthorIds)
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        foreach (var categoryId in dto.CategoryIds)
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });

        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Book created: {Id} {Title}", book.Id, book.Title);
        return (await GetBookByIdAsync(book.Id))!;
    }

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return null;

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var newAvailable = dto.TotalCopies - activeLoans;

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = Math.Max(0, newAvailable);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        _db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in dto.AuthorIds)
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });

        // Update categories
        _db.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in dto.CategoryIds)
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });

        await _db.SaveChangesAsync();
        return (await GetBookByIdAsync(id))!;
    }

    public async Task<(bool Success, string? Error)> DeleteBookAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return (false, "Book not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans) return (false, "Cannot delete book with active loans.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId).OrderByDescending(l => l.LoanDate);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(MapLoanToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(MapReservationToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    private static BookDto MapToDto(Book b) => new()
    {
        Id = b.Id, Title = b.Title, ISBN = b.ISBN, Publisher = b.Publisher,
        PublicationYear = b.PublicationYear, Description = b.Description,
        PageCount = b.PageCount, Language = b.Language,
        TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt,
        Authors = b.BookAuthors.Select(ba => new AuthorSummaryDto
        {
            Id = ba.Author.Id, FirstName = ba.Author.FirstName, LastName = ba.Author.LastName
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategorySummaryDto
        {
            Id = bc.Category.Id, Name = bc.Category.Name
        }).ToList()
    };

    internal static LoanDto MapLoanToDto(Loan l) => new()
    {
        Id = l.Id, BookId = l.BookId, BookTitle = l.Book.Title,
        PatronId = l.PatronId, PatronName = $"{l.Patron.FirstName} {l.Patron.LastName}",
        LoanDate = l.LoanDate, DueDate = l.DueDate, ReturnDate = l.ReturnDate,
        Status = l.Status, RenewalCount = l.RenewalCount, CreatedAt = l.CreatedAt
    };

    internal static ReservationDto MapReservationToDto(Reservation r) => new()
    {
        Id = r.Id, BookId = r.BookId, BookTitle = r.Book.Title,
        PatronId = r.PatronId, PatronName = $"{r.Patron.FirstName} {r.Patron.LastName}",
        ReservationDate = r.ReservationDate, ExpirationDate = r.ExpirationDate,
        Status = r.Status, QueuePosition = r.QueuePosition, CreatedAt = r.CreatedAt
    };
}
