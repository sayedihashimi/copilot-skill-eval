using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class BookService(LibraryDbContext context, ILogger<BookService> logger) : IBookService
{
    public async Task<PagedResult<BookListResponse>> GetBooksAsync(string? search, bool? available, string? sortBy, int page, int pageSize)
    {
        // No Include needed — projection via Select makes EF load only needed columns
        var query = context.Books
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.ISBN.ToLower().Contains(term) ||
                b.BookAuthors.Any(ba =>
                    ba.Author.FirstName.ToLower().Contains(term) ||
                    ba.Author.LastName.ToLower().Contains(term)) ||
                b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(term)));
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

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookListResponse
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                PublicationYear = b.PublicationYear,
                Language = b.Language,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                AuthorNames = b.BookAuthors.Select(ba => ba.Author.FirstName + " " + ba.Author.LastName).ToList(),
                CategoryNames = b.BookCategories.Select(bc => bc.Category.Name).ToList()
            })
            .ToListAsync();

        return new PagedResult<BookListResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BookResponse?> GetBookByIdAsync(int id)
    {
        // No Include needed — projection via Select makes EF load only needed columns
        return await context.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookResponse
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
                Authors = b.BookAuthors.Select(ba => new AuthorResponse
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    Biography = ba.Author.Biography,
                    BirthDate = ba.Author.BirthDate,
                    Country = ba.Author.Country,
                    CreatedAt = ba.Author.CreatedAt
                }).ToList(),
                Categories = b.BookCategories.Select(bc => new CategoryResponse
                {
                    Id = bc.Category.Id,
                    Name = bc.Category.Name,
                    Description = bc.Category.Description
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
    {
        if (await context.Books.AnyAsync(b => b.ISBN == request.ISBN))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Description = request.Description,
            PageCount = request.PageCount,
            Language = request.Language,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Books.Add(book);

        // Add author associations
        foreach (var authorId in request.AuthorIds)
        {
            if (!await context.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            context.BookAuthors.Add(new BookAuthor { Book = book, AuthorId = authorId });
        }

        // Add category associations
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            context.BookCategories.Add(new BookCategory { Book = book, CategoryId = categoryId });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);

        return (await GetBookByIdAsync(book.Id))!;
    }

    public async Task<BookResponse?> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        var book = await context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null) return null;

        if (await context.Books.AnyAsync(b => b.ISBN == request.ISBN && b.Id != id))
            throw new InvalidOperationException($"A book with ISBN '{request.ISBN}' already exists.");

        var activeLoans = await context.Loans.CountAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        var newAvailable = request.TotalCopies - activeLoans;
        if (newAvailable < 0)
            throw new InvalidOperationException($"Cannot reduce TotalCopies below the number of active loans ({activeLoans}).");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Description = request.Description;
        book.PageCount = request.PageCount;
        book.Language = request.Language;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = newAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        // Update author associations
        context.BookAuthors.RemoveRange(book.BookAuthors);
        foreach (var authorId in request.AuthorIds)
        {
            if (!await context.Authors.AnyAsync(a => a.Id == authorId))
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            context.BookAuthors.Add(new BookAuthor { BookId = id, AuthorId = authorId });
        }

        // Update category associations
        context.BookCategories.RemoveRange(book.BookCategories);
        foreach (var categoryId in request.CategoryIds)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == categoryId))
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            context.BookCategories.Add(new BookCategory { BookId = id, CategoryId = categoryId });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Updated book {BookId}", id);

        return (await GetBookByIdAsync(id))!;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await context.Books.FindAsync(id);
        if (book is null)
            throw new KeyNotFoundException($"Book with ID {id} not found.");

        var hasActiveLoans = await context.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new InvalidOperationException("Cannot delete a book with active loans.");

        context.Books.Remove(book);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted book {BookId}", id);
        return true;
    }

    public async Task<PagedResult<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize)
    {
        if (!await context.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = context.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.LoanDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse
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
                Status = l.Status.ToString(),
                RenewalCount = l.RenewalCount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<LoanResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize)
    {
        if (!await context.Books.AnyAsync(b => b.Id == bookId))
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        var query = context.Reservations
            .AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.BookId == bookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                PatronId = r.PatronId,
                PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status.ToString(),
                QueuePosition = r.QueuePosition,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ReservationResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    internal static LoanResponse MapLoanResponse(Loan l) => new()
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
        Status = l.Status.ToString(),
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
    };

    internal static ReservationResponse MapReservationResponse(Reservation r) => new()
    {
        Id = r.Id,
        BookId = r.BookId,
        BookTitle = r.Book.Title,
        PatronId = r.PatronId,
        PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
        ReservationDate = r.ReservationDate,
        ExpirationDate = r.ExpirationDate,
        Status = r.Status.ToString(),
        QueuePosition = r.QueuePosition,
        CreatedAt = r.CreatedAt
    };
}
