using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext db, ILogger<BookService> logger)
    : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAllAsync(
        string? search, bool? available, string? sortBy, string? sortDirection,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)) ||
                b.BookCategories.Any(bc =>
                    bc.Category.Name.ToLower().Contains(term)));
        }

        if (available.HasValue)
        {
            query = available.Value
                ? query.Where(b => b.AvailableCopies > 0)
                : query.Where(b => b.AvailableCopies == 0);
        }

        query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(b => b.Title),
            ("title", _) => query.OrderBy(b => b.Title),
            ("year", "desc") => query.OrderByDescending(b => b.PublicationYear),
            ("year", _) => query.OrderBy(b => b.PublicationYear),
            ("created", "desc") => query.OrderByDescending(b => b.CreatedAt),
            ("created", _) => query.OrderBy(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(MapToResponse).ToList();

        return PaginatedResponse<BookResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var book = await db.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return book is null ? null : MapToResponse(book);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        var existingIsbn = await db.Books.AnyAsync(b => b.ISBN == request.ISBN, ct);
        if (existingIsbn)
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

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

        foreach (var authorId in request.AuthorIds)
        {
            var authorExists = await db.Authors.AnyAsync(a => a.Id == authorId, ct);
            if (!authorExists)
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in request.CategoryIds)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == categoryId, ct);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        // Reload with navigation properties
        var created = await db.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == book.Id, ct);

        return MapToResponse(created);
    }

    public async Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        var duplicateIsbn = await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id, ct);
        if (duplicateIsbn)
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        // Calculate the difference in copies to adjust AvailableCopies
        var copyDiff = request.TotalCopies - book.TotalCopies;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copyDiff);
        book.UpdatedAt = DateTime.UtcNow;

        // Update authors
        book.BookAuthors.Clear();
        foreach (var authorId in request.AuthorIds)
        {
            var authorExists = await db.Authors.AnyAsync(a => a.Id == authorId, ct);
            if (!authorExists)
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update categories
        book.BookCategories.Clear();
        foreach (var categoryId in request.CategoryIds)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == categoryId, ct);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated book {BookId}", book.Id);

        // Reload
        var updated = await db.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == id, ct);

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new InvalidOperationException($"Cannot delete book '{book.Title}' because it has active loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted book {BookId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(
        int bookId, int page, int pageSize, CancellationToken ct)
    {
        var bookExists = await db.Books.AnyAsync(b => b.Id == bookId, ct);
        if (!bookExists)
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(LoanServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<LoanResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(
        int bookId, int page, int pageSize, CancellationToken ct)
    {
        var bookExists = await db.Books.AnyAsync(b => b.Id == bookId, ct);
        if (!bookExists)
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(ReservationServiceHelper.MapToResponse).ToList();

        return PaginatedResponse<ReservationResponse>.Create(responses, page, pageSize, totalCount);
    }

    private static BookResponse MapToResponse(Book b) =>
        new(b.Id, b.Title, b.ISBN, b.Publisher, b.PublicationYear,
            b.Description, b.PageCount, b.Language,
            b.TotalCopies, b.AvailableCopies,
            b.CreatedAt, b.UpdatedAt,
            b.BookAuthors.Select(ba => new BookAuthorResponse(
                ba.Author.Id, ba.Author.FirstName, ba.Author.LastName)).ToList(),
            b.BookCategories.Select(bc => new BookCategoryResponse(
                bc.Category.Id, bc.Category.Name)).ToList());
}

internal static class LoanServiceHelper
{
    public static LoanResponse MapToResponse(Loan l) =>
        new(l.Id, l.BookId, l.Book.Title, l.PatronId,
            $"{l.Patron.FirstName} {l.Patron.LastName}",
            l.LoanDate, l.DueDate, l.ReturnDate,
            l.Status, l.RenewalCount, l.CreatedAt);
}

internal static class ReservationServiceHelper
{
    public static ReservationResponse MapToResponse(Reservation r) =>
        new(r.Id, r.BookId, r.Book.Title, r.PatronId,
            $"{r.Patron.FirstName} {r.Patron.LastName}",
            r.ReservationDate, r.ExpirationDate,
            r.Status, r.QueuePosition, r.CreatedAt);
}
