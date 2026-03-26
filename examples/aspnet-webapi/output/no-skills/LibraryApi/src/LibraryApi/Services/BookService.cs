using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;

    public BookService(LibraryDbContext db) => _db = db;

    public async Task<PagedResult<BookDto>> GetAllAsync(string? search, bool? available, string? sortBy, string? sortDir, int page, int pageSize)
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
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(s) || ba.Author.LastName.ToLower().Contains(s)) ||
                b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(s)));
        }

        if (available == true)
            query = query.Where(b => b.AvailableCopies > 0);
        else if (available == false)
            query = query.Where(b => b.AvailableCopies == 0);

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            (_, "desc") => query.OrderByDescending(b => b.Title),
            _ => query.OrderBy(b => b.Title),
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateAsync(BookCreateDto dto)
    {
        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new BusinessRuleException($"A book with ISBN '{dto.ISBN}' already exists.");

        var book = new Book
        {
            Title = dto.Title, ISBN = dto.ISBN, Publisher = dto.Publisher,
            PublicationYear = dto.PublicationYear, Description = dto.Description,
            PageCount = dto.PageCount, Language = dto.Language,
            TotalCopies = dto.TotalCopies, AvailableCopies = dto.TotalCopies,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Books.Add(book);

        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found.");
            _db.BookAuthors.Add(new BookAuthor { Book = book, AuthorId = authorId });
        }
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            _db.BookCategories.Add(new BookCategory { Book = book, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();

        return (await GetByIdAsync(book.Id))!;
    }

    public async Task<BookDto?> UpdateAsync(int id, BookUpdateDto dto)
    {
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return null;

        if (await _db.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new BusinessRuleException($"A book with ISBN '{dto.ISBN}' already exists.");

        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (dto.TotalCopies < activeLoans)
            throw new BusinessRuleException($"Cannot reduce TotalCopies below the number of active loans ({activeLoans}).");

        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.AvailableCopies = dto.TotalCopies - activeLoans;
        book.TotalCopies = dto.TotalCopies;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        _db.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await _db.Authors.AnyAsync(a => a.Id == authorId))
                throw new NotFoundException($"Author with ID {authorId} not found.");
            _db.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        _db.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            _db.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) throw new NotFoundException($"Book with ID {id} not found.");
        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new BusinessRuleException("Cannot delete a book that has active loans.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId).OrderByDescending(l => l.LoanDate);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<List<ReservationDto>> GetBookReservationsAsync(int bookId)
    {
        if (!await _db.Books.AnyAsync(b => b.Id == bookId))
            throw new NotFoundException($"Book with ID {bookId} not found.");

        return await _db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => ReservationService.MapToDto(r))
            .ToListAsync();
    }

    private static BookDto MapToDto(Book b) => new()
    {
        Id = b.Id, Title = b.Title, ISBN = b.ISBN, Publisher = b.Publisher,
        PublicationYear = b.PublicationYear, Description = b.Description,
        PageCount = b.PageCount, Language = b.Language,
        TotalCopies = b.TotalCopies, AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt,
        Authors = b.BookAuthors.Select(ba => new AuthorSummaryDto { Id = ba.Author.Id, FirstName = ba.Author.FirstName, LastName = ba.Author.LastName }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategorySummaryDto { Id = bc.Category.Id, Name = bc.Category.Name }).ToList()
    };
}
