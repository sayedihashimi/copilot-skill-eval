using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? search, bool? available, string? sortBy, int page, int pageSize)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(term) || ba.Author.LastName.ToLower().Contains(term)) ||
                b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(term)));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(b => b.Title),
            "year" => query.OrderByDescending(b => b.PublicationYear),
            "available" => query.OrderByDescending(b => b.AvailableCopies),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(b => MapToResponse(b)).ToList();

        return new PaginatedResponse<BookResponse>(dtos, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<BookResponse?> GetBookByIdAsync(int id)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        return book is null ? null : MapToResponse(book);
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN))
        {
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");
        }

        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Description = request.Description,
            PageCount = request.PageCount,
            Language = request.Language ?? "English",
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies
        };

        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId))
            {
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            }
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId))
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            }
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        db.Books.Add(book);
        await db.SaveChangesAsync();

        logger.LogInformation("Created book {BookId}: {Title} (ISBN: {ISBN})", book.Id, book.Title, book.ISBN);

        return await GetBookByIdAsync(book.Id) ?? throw new InvalidOperationException("Failed to retrieve created book.");
    }

    public async Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id))
        {
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");
        }

        var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var copiesDiff = request.TotalCopies - book.TotalCopies;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.TotalCopies - activeLoans;
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId))
            {
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            }
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId))
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            }
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync();

        return await GetBookByIdAsync(id);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null)
        {
            throw new KeyNotFoundException($"Book with ID {id} not found.");
        }

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
        {
            throw new InvalidOperationException($"Cannot delete book '{book.Title}' because it has active loans.");
        }

        db.Books.Remove(book);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted book {BookId}: {Title}", id, book.Title);
        return true;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId))
        {
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");
        }

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName, l.LoanDate, l.DueDate,
                l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId))
        {
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");
        }

        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName, r.ReservationDate, r.ExpirationDate,
                r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    private static BookResponse MapToResponse(Book b) => new(
        b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description, b.PageCount,
        b.Language, b.TotalCopies, b.AvailableCopies, b.CreatedAt, b.UpdatedAt,
        b.BookAuthors.Select(ba => new BookAuthorResponse(ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
        b.BookCategories.Select(bc => new BookCategoryResponse(bc.Category.Id, bc.Category.Name)).ToList());
}
