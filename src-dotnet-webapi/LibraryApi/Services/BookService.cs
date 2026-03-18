using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService(LibraryDbContext db) : IBookService
{
    public async Task<PagedResponse<BookResponse>> GetAllAsync(
        string? search, string? category, bool? available,
        string? sortBy, string? sortDirection,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.Contains(term) ||
                b.BookAuthors.Any(ba => ba.Author.LastName.ToLower().Contains(term) || ba.Author.FirstName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category.ToLower()));
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
        return Paginate(responses, page, pageSize, totalCount);
    }

    public async Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        return new BookDetailResponse
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Publisher = book.Publisher,
            PublicationYear = book.PublicationYear,
            Description = book.Description,
            PageCount = book.PageCount,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Authors = book.BookAuthors.Select(ba => new AuthorSummaryResponse
            {
                Id = ba.Author.Id,
                FirstName = ba.Author.FirstName,
                LastName = ba.Author.LastName
            }).ToList(),
            Categories = book.BookCategories.Select(bc => new CategorySummaryResponse
            {
                Id = bc.Category.Id,
                Name = bc.Category.Name
            }).ToList()
        };
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct)
    {
        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN, ct))
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
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);

        return await GetResponseWithRelationsAsync(book.Id, ct);
    }

    public async Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await db.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null) return null;

        if (await db.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id, ct))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var activeLoanCount = await db.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active, ct);
        if (request.TotalCopies < activeLoanCount)
            throw new ArgumentException($"Cannot set TotalCopies below active loan count ({activeLoanCount}).");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language ?? "English";
        book.AvailableCopies += request.TotalCopies - book.TotalCopies;
        book.TotalCopies = request.TotalCopies;
        book.UpdatedAt = DateTime.UtcNow;

        book.BookAuthors.Clear();
        foreach (var authorId in request.AuthorIds)
        {
            if (!await db.Authors.AnyAsync(a => a.Id == authorId, ct))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            book.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        book.BookCategories.Clear();
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            book.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await db.SaveChangesAsync(ct);
        return await GetResponseWithRelationsAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Book with ID {id} not found.");

        if (book.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new InvalidOperationException("Cannot delete book with active or overdue loans.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Paginate(items.Select(MapLoanResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<List<ReservationResponse>> GetBookReservationsAsync(int bookId, CancellationToken ct)
    {
        if (!await db.Books.AnyAsync(b => b.Id == bookId, ct))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationResponse
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
            })
            .ToListAsync(ct);
    }

    private async Task<BookResponse> GetResponseWithRelationsAsync(int id, CancellationToken ct)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == id, ct);
        return MapToResponse(book);
    }

    private static BookResponse MapToResponse(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        ISBN = b.ISBN,
        Publisher = b.Publisher,
        PublicationYear = b.PublicationYear,
        Description = b.Description,
        PageCount = b.PageCount,
        Language = b.Language,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        Authors = b.BookAuthors.Select(ba => new AuthorSummaryResponse
        {
            Id = ba.Author.Id,
            FirstName = ba.Author.FirstName,
            LastName = ba.Author.LastName
        }).ToList(),
        Categories = b.BookCategories.Select(bc => new CategorySummaryResponse
        {
            Id = bc.Category.Id,
            Name = bc.Category.Name
        }).ToList()
    };

    private static LoanResponse MapLoanResponse(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        BookISBN = l.Book.ISBN,
        PatronId = l.PatronId,
        PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        Status = l.Status,
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
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
