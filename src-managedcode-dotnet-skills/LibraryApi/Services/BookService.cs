using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService(LibraryDbContext context, ILogger<BookService> logger) : IBookService
{
    public async Task<PagedResult<BookDto>> GetAllAsync(int page, int pageSize)
    {
        var totalCount = await context.Books.CountAsync();
        var items = await context.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return new PagedResult<BookDto>(items, totalCount, page, pageSize);
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await context.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        return book is null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto)
    {
        if (await context.Books.AnyAsync(b => b.ISBN == dto.ISBN))
            throw new InvalidOperationException($"A book with ISBN '{dto.ISBN}' already exists.");

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
        {
            if (!await context.Authors.AnyAsync(a => a.Id == authorId))
                throw new InvalidOperationException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == categoryId))
                throw new InvalidOperationException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        context.Books.Add(book);
        await context.SaveChangesAsync();

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        // Reload with navigation properties
        var created = await context.Books
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id);

        return MapToDto(created);
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto)
    {
        var book = await context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        if (await context.Books.AnyAsync(b => b.ISBN == dto.ISBN && b.Id != id))
            throw new InvalidOperationException($"A book with ISBN '{dto.ISBN}' already exists.");

        var copiesDiff = dto.TotalCopies - book.TotalCopies;
        book.Title = dto.Title;
        book.ISBN = dto.ISBN;
        book.Publisher = dto.Publisher;
        book.PublicationYear = dto.PublicationYear;
        book.Description = dto.Description;
        book.PageCount = dto.PageCount;
        book.Language = dto.Language;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in dto.AuthorIds)
        {
            if (!await context.Authors.AnyAsync(a => a.Id == authorId))
                throw new InvalidOperationException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in dto.CategoryIds)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == categoryId))
                throw new InvalidOperationException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Updated book {BookId}", id);

        // Reload with navigation properties
        var updated = await context.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == id);

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await context.Books
            .Include(b => b.Loans.Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return false;

        if (book.Loans.Count > 0)
            throw new InvalidOperationException("Cannot delete book with active or overdue loans.");

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted book {BookId}", id);
        return true;
    }

    public async Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await context.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = context.Loans
            .AsNoTracking()
            .Where(l => l.BookId == bookId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PagedResult<LoanDto>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        if (!await context.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = context.Reservations
            .AsNoTracking()
            .Where(r => r.BookId == bookId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.QueuePosition)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PagedResult<ReservationDto>(items, totalCount, page, pageSize);
    }

    private static BookDto MapToDto(Book b) => new(
        b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description,
        b.PageCount, b.Language, b.TotalCopies, b.AvailableCopies, b.CreatedAt, b.UpdatedAt,
        b.BookAuthors.Select(ba => new AuthorDto(
            ba.Author.Id, ba.Author.FirstName, ba.Author.LastName,
            ba.Author.Biography, ba.Author.BirthDate, ba.Author.Country, ba.Author.CreatedAt
        )).ToList(),
        b.BookCategories.Select(bc => new CategoryDto(
            bc.Category.Id, bc.Category.Name, bc.Category.Description
        )).ToList());
}
