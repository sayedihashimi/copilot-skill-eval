using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetBooksAsync(
        string? search, bool? available, string? sortBy, string? sortOrder,
        int page, int pageSize, CancellationToken ct)
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
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(term) || ba.Author.LastName.ToLower().Contains(term)) ||
                b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(term)));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            (_, "desc") => query.OrderByDescending(b => b.Title),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);
        var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var items = books.Select(b => new BookResponse(
            b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear, b.Description,
            b.PageCount, b.Language, b.TotalCopies, b.AvailableCopies,
            b.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
            b.BookCategories.Select(bc => bc.Category.Name).ToList(),
            b.CreatedAt, b.UpdatedAt
        )).ToList();

        return new PaginatedResponse<BookResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<BookDetailResponse?> GetBookByIdAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        return new BookDetailResponse(
            book.Id, book.Title, book.ISBN, book.Publisher, book.PublicationYear,
            book.Description, book.PageCount, book.Language, book.TotalCopies, book.AvailableCopies,
            book.BookAuthors.Select(ba => new AuthorResponse(
                ba.Author.Id, ba.Author.FirstName, ba.Author.LastName,
                ba.Author.Biography, ba.Author.BirthDate, ba.Author.Country, ba.Author.CreatedAt
            )).ToList(),
            book.BookCategories.Select(bc => new CategoryResponse(bc.Category.Id, bc.Category.Name, bc.Category.Description)).ToList(),
            book.CreatedAt, book.UpdatedAt
        );
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request, CancellationToken ct)
    {
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
            AvailableCopies = request.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        // Add authors
        if (request.AuthorIds is { Count: > 0 })
        {
            foreach (var authorId in request.AuthorIds)
            {
                db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }
            await db.SaveChangesAsync(ct);
        }

        // Add categories
        if (request.CategoryIds is { Count: > 0 })
        {
            foreach (var categoryId in request.CategoryIds)
            {
                db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }
            await db.SaveChangesAsync(ct);
        }

        // Reload with relations
        var created = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id, ct);

        logger.LogInformation("Book created: {Id} - {Title}", book.Id, book.Title);

        return new BookResponse(
            created.Id, created.Title, created.ISBN, created.Publisher, created.PublicationYear,
            created.Description, created.PageCount, created.Language, created.TotalCopies, created.AvailableCopies,
            created.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
            created.BookCategories.Select(bc => bc.Category.Name).ToList(),
            created.CreatedAt, created.UpdatedAt
        );
    }

    public async Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";

        // Adjust available copies based on total copies change
        var copiesDiff = request.TotalCopies - book.TotalCopies;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        if (request.AuthorIds is not null)
        {
            db.BookAuthors.RemoveRange(book.BookAuthors);
            foreach (var authorId in request.AuthorIds)
            {
                db.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }
        }

        // Update categories
        if (request.CategoryIds is not null)
        {
            db.BookCategories.RemoveRange(book.BookCategories);
            foreach (var categoryId in request.CategoryIds)
            {
                db.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }
        }

        await db.SaveChangesAsync(ct);

        var updated = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id, ct);

        return new BookResponse(
            updated.Id, updated.Title, updated.ISBN, updated.Publisher, updated.PublicationYear,
            updated.Description, updated.PageCount, updated.Language, updated.TotalCopies, updated.AvailableCopies,
            updated.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}").ToList(),
            updated.BookCategories.Select(bc => bc.Category.Name).ToList(),
            updated.CreatedAt, updated.UpdatedAt
        );
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book is null) return (false, false);
        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue)) return (true, true);

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Book deleted: {Id}", id);
        return (true, false);
    }

    public async Task<IReadOnlyList<LoanResponse>?> GetBookLoansAsync(int bookId, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct)) return null;

        return await db.Loans.AsNoTracking()
            .Where(l => l.BookId == bookId)
            .Include(l => l.Book).Include(l => l.Patron)
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReservationResponse>?> GetBookReservationsAsync(int bookId, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct)) return null;

        return await db.Reservations.AsNoTracking()
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .Include(r => r.Book).Include(r => r.Patron)
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);
    }
}
